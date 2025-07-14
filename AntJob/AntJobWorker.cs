using NewLife;
using NewLife.Log;
using NewLife.Model;
#if !NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace AntJob;

class AntJobWorker(Scheduler scheduler, IServiceProvider serviceProvider) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 实例化调度器
        scheduler.ServiceProvider = serviceProvider;
        scheduler.Log = serviceProvider.GetService<ILog>();
        scheduler.Tracer = serviceProvider.GetService<ITracer>();

        var set = serviceProvider.GetService<AntSetting>();
        set ??= AntSetting.Current;
        scheduler.Join(set);

        // 添加作业
        //scheduler.AddHandler<HelloJob>();

        // 启动调度引擎，调度器内部多线程处理
        scheduler.StartAsync();

        return TaskEx.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        scheduler.TryDispose();
        scheduler = null;

        return base.StopAsync(cancellationToken);
    }
}