using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>作业错误</summary>
[AntArea]
[DisplayName("作业错误")]
[Menu(0, false)]
public class JobErrorController : AntEntityController<JobError>
{
    static JobErrorController()
    {
        ListFields.RemoveField("DataTime", "End");
        ListFields.AddListField("Message", null, "TraceId");

        ListFields.TraceUrl();
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<JobError> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var appid = p["appid"].ToInt(-1);
        var jobid = p["JobID"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();
        var client = p["Client"];

        return JobError.Search(appid, jobid, client, start, end, p["q"], p);
    }
}