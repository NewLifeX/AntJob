using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using AntJob;
using NewLife;
using NewLife.Http;
using NewLife.Remoting;
using NewLife.Security;

namespace HisAgent;

[DisplayName("定时欢迎")]
[Description("简单的定时任务")]
internal class HelloJob : Handler
{
    public HelloJob()
    {
        Job.Cron = "7/30 * * * * ?";
        //MaxInactiveTime = 60;
    }

    public override Int32 Execute(JobContext ctx)
    {
        using var span = Tracer?.NewSpan("HelloJob", ctx.Task.DataTime);

        // 当前任务时间
        var time = ctx.Task.DataTime;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);
        if (!ctx.Task.Data.IsNullOrEmpty()) WriteLog("数据：{0}", ctx.Task.Data);

        // 成功处理数据量
        return 1;
    }

    public override async Task<Int32> ExecuteAsync(JobContext ctx)
    {
        using var span = Tracer?.NewSpan("HelloJob", ctx.Task.DataTime);

        // 当前任务时间
        var time = ctx.Task.DataTime;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);
        if (!ctx.Task.Data.IsNullOrEmpty()) WriteLog("数据：{0}", ctx.Task.Data);

        var state = Rand.NextString(16);
        var http = new HttpClient { BaseAddress = new Uri("https://newlifex.com") };
        http.SetUserAgent();
        var rs = await http.GetAsync<IDictionary<String, Object>>("/cube/info", new { state });

        //await Task.Delay(90_000);
        if (rs.TryGetValue("state", out var value) && value is String str)
        {
            WriteLog("返回状态：{0}", str);
            Trace.Assert(state == str, "返回状态不一致");
        }

        // 成功处理数据量
        return 1;
    }
}