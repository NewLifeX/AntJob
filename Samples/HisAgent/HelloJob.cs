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
        // 今天零点开始，每10秒一次
        var job = Job;
        job.Start = DateTime.Today;
        job.Step = 10;
    }

    protected override Int32 Execute(JobContext ctx)
    {
        // 当前任务时间
        var time = ctx.Task.Time;
        WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);

        // 成功处理数据量
        return 1;
    }
}