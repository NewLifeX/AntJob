using System;
using AntJob.Extensions;
using AntJob.Handlers;
using AntJob.Providers;
using NewLife;
using NewLife.Agent;

namespace AntJob.Agent
{
    class Program
    {
        static void Main(String[] args) => new MyService().Main();
    }

    /// <summary>服务类。名字可以自定义</summary>
    class MyService : ServiceBase
    {
        public MyService()
        {
            ServiceName = "AntAgent";

            // 注册菜单，在控制台菜单中按 t 可以执行Test函数，主要用于临时处理数据
            AddMenu('t', "数据测试", Test);
        }

        private Scheduler _Scheduler;
        /// <summary>服务启动</summary>
        /// <remarks>
        /// 安装Windows服务后，服务启动会执行一次该方法。
        /// 控制台菜单按5进入循环调试也会执行该方法。
        /// </remarks>
        protected override void StartWork(String reason)
        {
            var set = Setting.Current;

            // 实例化调度器
            var sc = new Scheduler();

            // 使用分布式调度引擎替换默认的本地文件调度
            sc.Provider = new NetworkJobProvider
            {
                Debug = set.Debug,
                Server = set.Server,
                AppID = set.AppID,
                Secret = set.Secret,
            };

            // 添加作业处理器
            sc.Handlers.Add(new CSharpHandler());
            sc.Handlers.Add(new SqlHandler());

            // 启动调度引擎，调度器内部多线程处理
            sc.Start();

            _Scheduler = sc;

            base.StartWork(reason);
        }

        /// <summary>服务停止</summary>
        /// <remarks>
        /// 安装Windows服务后，服务停止会执行该方法。
        /// 控制台菜单按5进入循环调试，任意键结束时也会执行该方法。
        /// </remarks>
        protected override void StopWork(String reason)
        {
            base.StopWork(reason);

            _Scheduler.TryDispose();
            _Scheduler = null;
        }

        /// <summary>数据测试，菜单t，默认文件调度</summary>
        public void Test()
        {
        }
    }
}