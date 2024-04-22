using System;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using Microsoft.Extensions.Hosting;
using NewLife;
using NewLife.Log;

namespace HisAgent;

public class JobHost : BackgroundService
{
    private Scheduler _scheduler;
    private readonly IServiceProvider _serviceProvider;

    public JobHost(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var set = AntSetting.Current;

        // 实例化调度器
        var scheduler = new Scheduler
        {
            ServiceProvider = _serviceProvider,
            Log = XTrace.Log,
        };

        scheduler.Join(set.Server, set.AppID, set.Secret, set.Debug);

        // 添加作业
        scheduler.AddHandler<HelloJob>();
        scheduler.AddHandler<BuildPatient>();
        scheduler.AddHandler<BuildWill>();

        // 启动调度引擎，调度器内部多线程处理
        scheduler.Start();
        _scheduler = scheduler;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.TryDispose();
        _scheduler = null;

        return Task.CompletedTask;
    }
}
