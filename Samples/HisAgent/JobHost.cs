using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using AntJob.Providers;
using Microsoft.Extensions.Hosting;
using NewLife;
using NewLife.Configuration;
using NewLife.Log;

namespace HisAgent
{
    public class JobHost : BackgroundService
    {
        //private readonly IConfigProvider _config;
        //private readonly ITracer _tracer;
        private Scheduler _scheduler;

        public JobHost()
        {
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var set = AntSetting.Current;

            // 实例化调度器
            var sc = new Scheduler
            {
                // 使用分布式调度引擎替换默认的本地文件调度
                Provider = new NetworkJobProvider
                {
                    Server = set.Server,
                    AppID = set.AppID,
                    Secret = set.Secret,
                    Debug = false
                }
            };

            // 添加作业
            sc.AddHandler<HelloJob>();


            // 启动调度引擎，调度器内部多线程处理
            sc.Start();
            _scheduler = sc;

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _scheduler.TryDispose();
            _scheduler = null;

            return Task.CompletedTask;
        }
    }
}
