using System.ComponentModel;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Server.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Data;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>作业任务</summary>
[AntArea]
[DisplayName("作业任务")]
[Menu(0, false)]
public class JobTaskController : AntEntityController<JobTask>
{
    private readonly JobService _jobService;

    static JobTaskController()
    {
        LogOnChange = true;

        ListFields.RemoveField("Server", "ProcessID");
        ListFields.RemoveCreateField();

        {
            var df = ListFields.GetField("Success") as ListField;
            df.AddService(new ColorNumberField { Color = "green" });
            //df.GetValue = e => $"<font color=green><b>{(e as IModel)["Success"]:n0}</b></font>";
        }
        {
            var df = ListFields.GetField("Error");
            df.DataVisible = e => (e as JobTask).Error > 0;
            df.AddService(new ColorNumberField { Color = "red" });
        }
        {
            var df = ListFields.GetField("Status");
            df.AddService(new MyStatusField());
        }

        ListFields.TraceUrl();
    }

    class MyStatusField : ILinkExtend
    {
        public String Resolve(DataField field, IModel data)
        {
            var task = data as JobTask;
            return task.Status switch
            {
                JobStatus.就绪 => $"<font color=text-center>{task.Status}</font>",
                JobStatus.抽取中 => $"<font color=warning>{task.Status}</font>",
                JobStatus.处理中 => $"<font color=info>{task.Status}</font>",
                JobStatus.错误 => $"<font color=danger>{task.Status}</font>",
                JobStatus.完成 => $"<font color=success>{task.Status}</font>",
                JobStatus.取消 => $"<font color=active>{task.Status}</font>",
                JobStatus.延迟 => $"<font color=active>{task.Status}</font>",
                _ => $"<font color=info>{task.Status}</font>",
            };
        }
    }

    class ColorField : ILinkExtend
    {
        public String Color { get; set; }

        public Func<Object, String> GetValue;

        public String Resolve(DataField field, IModel data)
        {
            var value = data[field.Name];
            if (GetValue != null) value = GetValue(value);
            return $"<font color={Color}><b>{value}</b></font>";
        }
    }

    class ColorNumberField : ILinkExtend
    {
        public String Color { get; set; }

        public String Resolve(DataField field, IModel data)
        {
            var value = data[field.Name];
            return $"<font color={Color}><b>{value:n0}</b></font>";
        }
    }

    public JobTaskController(JobService jobService) => _jobService = jobService;

    protected override FieldCollection OnGetFields(ViewKinds kind, Object model)
    {
        var fs = base.OnGetFields(kind, model);
        if (kind == ViewKinds.List)
        {
            var jobId = GetRequest("jobId").ToInt();
            if (jobId > 0) fs.RemoveField("JobID");
        }

        return fs;
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<JobTask> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var id = p["id"].ToInt(-1);
        var jobid = p["JobID"].ToInt(-1);
        var appid = p["AppID"].ToInt(-1);
        var status = (JobStatus)p["Status"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();
        var client = p["Client"];

        if (jobid > 0)
        {
            ListFields.RemoveField("JobID");
        }

        return JobTask.Search(id, appid, jobid, status, start, end, client, p["q"], p);
    }

    /// <summary>修改状态</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult Set(Int32 id = 0)
    {
        var rs = 0;
        var ids = GetRequest("keys").SplitAsInt();
        foreach (var item in ids)
        {
            var task = JobTask.FindByID(item);
            if (task != null)
            {
                task.Status = JobStatus.取消;
                if (task.Times >= 10) task.Times = 0;

                rs += task.Save();

                // 提醒调度，马上放行
                _jobService.SetDelay(task.JobID, DateTime.Now);
            }
        }

        return JsonRefresh($"操作成功！rs={rs}");
    }
}