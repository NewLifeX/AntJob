using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用在线</summary>
[AntArea]
[DisplayName("应用在线")]
[Menu(80)]
public class AppOnlineController : AntEntityController<AppOnline>
{
    static AppOnlineController()
    {
        //AppOnline.Meta.Table.DataTable.InsertOnly = true;

        ListFields.RemoveField("End");

        ListFields.TraceUrl();
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<AppOnline> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var appid = p["appid"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return AppOnline.Search(appid, start, end, p["q"], p);
    }
}