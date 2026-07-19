using NewLife;
using NewLife.Log;
using NewLife.Model;
#if !NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace AntJob;

/// <summary>蚂蚁调度后台服务。将 Scheduler 作为 BackgroundService 托管到 DI 容器</summary>
/// <remarks>
/// 由 AddAntJob() 扩展方法自动注册，负责：
/// 1. 从 DI 容器获取 AntSetting/AntSetting
/// 2. 初始化 Scheduler（Log/Tracer/Setting）
/// 3. 调用 StartAsync() 异步连接调度中心
/// </remarks>
internal class AntJobWorker(Scheduler scheduler, IServiceProvider serviceProvider) : BackgroundService
{
    /// <summary>异步启动。初始化调度器并启动连接</summary>
    /// <param name="stoppingToken">停止令牌</param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 实例化调度器
        scheduler.ServiceProvider = serviceProvider;
        scheduler.Log = serviceProvider.GetService<ILog>();
        scheduler.Tracer = serviceProvider.GetService<ITracer>();

        var set = serviceProvider.GetService<AntSetting>();
        set ??= AntSetting.Current;
        scheduler.Setting = set;

        // 启动调度引擎，调度器内部多线程处理
        scheduler.StartAsync();

        return TaskEx.CompletedTask;
    }

    /// <summary>异步停止。销毁调度器</summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        scheduler.TryDispose();
        scheduler = null;

        return base.StopAsync(cancellationToken);
    }
}