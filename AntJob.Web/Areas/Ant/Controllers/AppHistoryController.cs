using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用历史</summary>
[AntArea]
[DisplayName("应用历史")]
[Menu(0, false)]
public class AppHistoryController : AntEntityController<AppHistory>
{
    static AppHistoryController()
    {
        //AppHistory.Meta.Table.DataTable.InsertOnly = true;

        ListFields.RemoveField("Id", "Version", "CompileTime", "");
        ListFields.AddListField("Remark", null, "TraceId");

        ListFields.TraceUrl();
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<AppHistory> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var appid = p["appid"].ToInt(-1);
        var act = p["action"];
        var success = p["success"]?.ToBoolean();
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return AppHistory.Search(appid, act, success, start, end, p["q"], p);
    }
}