using System;
using AntJob;
using NewLife.Log;

namespace HelloWork
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            XTrace.UseConsole();

            // 实例化调度器
            var sc = new Scheduler();

            // 添加作业处理器
            sc.Jobs.Add(new HelloJob());

            // 启动调度引擎，调度器内部多线程处理
            sc.Start();

            Console.WriteLine("OK!");
            Console.ReadKey();
        }
    }
}