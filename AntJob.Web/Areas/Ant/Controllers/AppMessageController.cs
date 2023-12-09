using System;
using System.Collections.Generic;
using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用消息</summary>
[AntArea]
[DisplayName("应用消息")]
public class AppMessageController : EntityController<AppMessage>
{
    //static AppMessageController() => MenuOrder = 49;

    static AppMessageController() => ListFields.TraceUrl();

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
}