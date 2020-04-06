using System;
using AntJob;

namespace HelloWork
{
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
            var time = ctx.Task.Start;
            WriteLog("新生命蚂蚁调度系统！当前任务时间：{0}", time);

            // 成功处理数据量
            return 1;
        }
    }
}