using System;
using System.ComponentModel;
using AntJob;
using NewLife;
using NewLife.Security;

namespace HisAgent;

[DisplayName("定时欢迎")]
[Description("简单的定时任务")]
internal class HelloJob : Handler
{
    public HelloJob()
    {
        Job.Cron = "7/30 * * * * ?";
    }

    public override Int32 Execute(JobContext ctx)
    {
        using var span = Tracer?.NewSpan("HelloJob", ctx.Task.DataTime);

        // 当前任务时间
        var time = ctx.Task.DataTime;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);
        if (!ctx.Task.Data.IsNullOrEmpty()) WriteLog("数据：{0}", ctx.Task.Data);

        //// 一定几率抛出异常
        //if (Rand.Next(2) == 0) throw new Exception("Error");

        // 成功处理数据量
        return 1;
    }
}