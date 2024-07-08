using System;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using NewLife;
using NewLife.Log;
using NewLife.Model;

namespace HisAgent;

public class JobHost : BackgroundService
{
    private Scheduler _scheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly AntSetting _setting;

    public JobHost(IServiceProvider serviceProvider, AntSetting setting)
    {
        _serviceProvider = serviceProvider;
        _setting = setting;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 实例化调度器
        var scheduler = new Scheduler
        {
            ServiceProvider = _serviceProvider,
            Log = XTrace.Log,
        };

        scheduler.Join(_setting);

        // 添加作业
        scheduler.AddHandler<HelloJob>();
        scheduler.AddHandler<BuildPatient>();
        scheduler.AddHandler<BuildWill>();

        // 启动调度引擎，调度器内部多线程处理
        scheduler.StartAsync();
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
