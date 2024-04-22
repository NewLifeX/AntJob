using System;
using System.ComponentModel;
using AntJob;

namespace HisAgent;

[DisplayName("定时欢迎")]
[Description("简单的定时任务")]
internal class HelloJob : Handler
{
    public HelloJob()
    {
        Job.Cron = "7/30 * * * * ?";
    }

    protected override Int32 Execute(JobContext ctx)
    {
        using var span = Tracer?.NewSpan("HelloJob", ctx.Task.Time);

        // 当前任务时间
        var time = ctx.Task.Time;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);

        // 成功处理数据量
        return 1;
    }
}