using System;
using AntJob;
using AntJob.Providers;
using NewLife.Log;

namespace HelloWork
{
    class Program
    {
        private static void Main(String[] args)
        {
            XTrace.UseConsole();

            var set = Setting.Current;

            // 实例化调度器
            var sc = new Scheduler();

            // 使用分布式调度引擎替换默认的本地文件调度
            sc.Provider = new NetworkJobProvider
            {
                Server = set.Server,
                AppID = set.AppID,
                Secret = set.Secret,
            };

            // 添加作业处理器
            sc.Jobs.Add(new HelloJob());

            // 启动调度引擎，调度器内部多线程处理
            sc.Start();

            Console.WriteLine("OK!");
            Console.ReadKey();
        }
    }

    class HelloJob : Handler
    {
        public HelloJob()
        {
            // 今天零点开始，每5分钟一次
            var job = Job;
            job.Start = DateTime.Today;
            job.Step = 5 * 60;
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