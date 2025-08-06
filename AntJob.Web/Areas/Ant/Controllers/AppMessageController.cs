using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用消息</summary>
[AntArea]
[DisplayName("应用消息")]
[Menu(70)]
public class AppMessageController : AntEntityController<AppMessage>
{
    static AppMessageController()
    {
        LogOnChange = true;

        ListFields.RemoveField("CreateIP", "UpdateIP");

        {
            var df = ListFields.AddListField("Data", null, "Topic");
            df.TextAlign = TextAligns.Nowrap;
        }

        ListFields.TraceUrl();
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<AppMessage> Search(Pager p)
    {
        var appid = p["appid"].ToInt(-1);
        var jobid = p["JobID"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return AppMessage.Search(appid, jobid, start, end, p["q"], p);
    }

    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ClearDelay()
    {
        var rs = 0;
        foreach (var item in SelectKeys)
        {
            var entity = AppMessage.FindById(item.ToLong());
            if (entity != null)
            {
                entity.DelayTime = DateTime.MinValue;
                rs += entity.Update();
            }
        }

        return JsonRefresh($"操作成功{rs}个");
    }

    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult SetDelay(DateTime delayTime)
    {
        if (delayTime.Year < 2000) throw new ArgumentNullException(nameof(delayTime));

        var rs = 0;
        foreach (var item in SelectKeys)
        {
            var entity = AppMessage.FindById(item.ToLong());
            if (entity != null)
            {
                entity.DelayTime = delayTime;

                rs += entity.Update();
            }
        }

        return JsonRefresh($"操作成功{rs}个");
    }
}