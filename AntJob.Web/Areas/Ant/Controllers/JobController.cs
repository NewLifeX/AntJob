using System.ComponentModel;
using AntJob.Data;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.ViewModels;
using NewLife.Data;
using NewLife.Security;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>作业</summary>
[AntArea]
[DisplayName("作业")]
[Menu(0, false)]
public class JobController : AntEntityController<Job>
{
    static JobController()
    {
        LogOnChange = true;

        ListFields.RemoveField("ClassName", "Step", "Cron", "Topic", "MessageCount", "Time", "End");
        ListFields.RemoveField("Times", "Speed");
        ListFields.RemoveField("MaxError", "MaxRetry", "MaxTime", "MaxRetain", "MaxIdle", "ErrorDelay", "Deadline");
        ListFields.RemoveCreateField().RemoveUpdateField();
        ListFields.AddListField("UpdateTime");

        {
            var df = ListFields.GetField("Name") as ListField;
            df.Url = "/Ant/JobTask?appid={AppID}&jobId={ID}";
        }
        //{
        //    var df = ListFields.AddListField("Task", "Enable");
        //    df.DisplayName = "任务";
        //    df.Url = "/Ant/JobTask?appid={AppID}&jobId={ID}";
        //}
        {
            var df = ListFields.AddListField("Title", null, "Mode");
            df.Header = "下一次/Cron/主题";
            df.HeaderTitle = "Cron格式，秒+分+时+天+月+星期+年";
            df.AddService(new MyTextField());
        }
        {
            var df = ListFields.GetField("DataTime") as ListField;
            //df.GetClass = e => "text-center text-primary font-weight-bold";
            df.AddService(new ColorField { Color = "Magenta", GetValue = e => ((DateTime)e).ToFullString("") });
        }
        //{
        //    var df = ListFields.GetField("Step");
        //    df.DataVisible = e => (e as Job).Mode == JobModes.Data;
        //}
        {
            var df = ListFields.GetField("BatchSize");
            df.DataVisible = e => (e as Job).Mode != JobModes.Time;
        }
        //{
        //    var df = ListFields.GetField("MaxTask");
        //    df.DataVisible = e => (e as Job).Mode != JobModes.Message;
        //}
        {
            var df = ListFields.GetField("Success");
            df.AddService(new ColorNumberField { Color = "green" });
        }
        {
            var df = ListFields.GetField("Error");
            df.AddService(new ColorNumberField { Color = "red" });
        }
        {
            var df = ListFields.GetField("LastStatus") as ListField;
            df.GetClass = e =>
            {
                var job = e as Job;
                return job.LastStatus switch
                {
                    JobStatus.就绪 => "text-center",
                    JobStatus.抽取中 => "text-center info",
                    JobStatus.处理中 => "text-center warning",
                    JobStatus.错误 => "text-center danger",
                    JobStatus.完成 => "text-center success",
                    JobStatus.取消 => "text-center active",
                    JobStatus.延迟 => "text-center active",
                    _ => "",
                };
            };
        }
    }

    class MyTextField : ILinkExtend
    {
        public String Resolve(DataField field, IModel data)
        {
            var job = data as Job;
            return job.Mode switch
            {
                JobModes.Data => $"<font color=blue><b>{job.DataTime.ToFullString("")}</b></font>",
                JobModes.Time => $"<font color=DarkVoilet><b>{job.Cron}</b></font>",
                JobModes.Message => $"<font color=green><b>{job.Topic}</b></font>",
                //JobModes.CSharp => "[C#]" + job.Time.ToFullString(""),
                //JobModes.Sql => "[Sql]" + job.Time.ToFullString(""),
                _ => $"<b>{job.DataTime.ToFullString("")}</b>",
            };
        }
    }

    class ColorField : ILinkExtend
    {
        public String Color { get; set; }

        public Func<Object, String> GetValue;

        public String Resolve(DataField field, IModel data)
        {
            if (data is Job job)
            {
                if (job.Mode == JobModes.Message) return "";
                if (job.Mode == JobModes.Data) return $"+{TimeSpan.FromSeconds(job.Step)}";
            }

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

    protected override FieldCollection OnGetFields(ViewKinds kind, Object model)
    {
        var fs = base.OnGetFields(kind, model);
        if (model is not Job job) return fs;

        if (kind is ViewKinds.EditForm or ViewKinds.Detail)
        {
            // Cron/Topic/MessageCount/End/Step/Offset/BatchSize
            switch (job.Mode)
            {
                case JobModes.Data:
                    fs.RemoveField("Topic", "MessageCount", "Cron");
                    break;
                case JobModes.Time:
                    fs.RemoveField("Topic", "MessageCount", "Step", "BatchSize");
                    break;
                case JobModes.Message:
                    fs.RemoveField("Cron", "End", "Step", "Offset");
                    break;
                default:
                    break;
            }
        }
        else if (kind is ViewKinds.AddForm)
        {
            fs.RemoveField("Cron", "Topic", "MessageCount", "End", "Step", "Offset", "BatchSize");
        }

        return fs;
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<Job> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var id = p["ID"].ToInt(-1);
        var appid = p["appid"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();
        var mode = p["Mode"].ToInt(-1);

        return Job.Search(id, appid, start, end, mode, p["q"], p);
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ResetTime(Int32 days = 0)
    {
        var ids = GetRequest("keys").SplitAsInt();
        var st = GetRequest("sday").ToDateTime();
        var et = GetRequest("eday").ToDateTime();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            dt?.ResetTime(days, st, et);
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>完全重置</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ResetOther()
    {
        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            dt?.ResetOther();
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>设置偏移</summary>
    /// <param name="offset">偏移</param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult SetOffset(Int32 offset)
    {
        if (offset < 0) offset = 15;

        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            if (dt != null)
            {
                dt.Offset = offset;
                dt.Save();
            }
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>清空错误数</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ClearError()
    {
        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            if (dt != null)
            {
                dt.Error = 0;
                dt.Save();
            }
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>克隆一个作业</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult Clone(Int32 id)
    {
        var job = Job.FindByID(id);
        if (job == null) return Index();

        // 拷贝一次对象，避免因为缓存等原因修改原来的数据
        job = job.Clone() as Job;

        // 随机名称，插入新行
        job.ID = 0;
        job.Name = Rand.NextString(8);
        job.Enable = false;
        job.Insert();

        // 跳转到编辑页，这里时候已经得到新的自增ID
        return Edit(job.ID + "");
    }
}