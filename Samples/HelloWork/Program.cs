using System;
using AntJob;
using NewLife.Log;

namespace HelloWork
{
    class Program
    {
        private static void Main(String[] args)
        {
            XTrace.UseConsole();

            // 手工实例化调度器，调度中心地址来自 AntSetting.Current
            var sc = new Scheduler { Setting = AntSetting.Current };

            // 添加作业处理器
            sc.Handlers.Add(new HelloJob());

            // 启动调度引擎，连接失败时按周期重试
            sc.StartAsync();

            Console.WriteLine("OK!");
            Console.ReadKey();

            sc.Stop();
        }
    }
}