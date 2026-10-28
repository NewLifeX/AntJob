using System;
using AntJob;
using AntJob.Data;

namespace HelloWork
{
    internal class HelloJob : Handler
    {
        public HelloJob()
        {
            // 定时调度，每10秒一个时间片
            var job = Job;
            job.Mode = JobModes.Time;
            job.DataTime = DateTime.Today;
            job.Step = 10;
        }

        public override Int32 Execute(JobContext ctx)
        {
            // 当前任务时间
            var time = ctx.Task.DataTime;
            WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);

            // 成功处理数据量
            return 1;
        }
    }
}