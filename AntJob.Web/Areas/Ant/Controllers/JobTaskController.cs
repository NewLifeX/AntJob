using System.ComponentModel;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Server.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
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

        ListFields.TraceUrl();
    }

    public JobTaskController(JobService jobService) => _jobService = jobService;

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