using NewLife.Configuration;
using NewLife.Log;
using NewLife.Model;

namespace AntJob;

/// <summary>蚂蚁调度依赖注入</summary>
public static class AntJobExtensions
{
    /// <summary>注入AndJob</summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static Scheduler AddAntJob(this IObjectContainer services)
    {
        // 实例化调度器
        var scheduler = new Scheduler();

        services.AddBasic();
        services.AddSingleton(scheduler);

        services.AddHostedService<AntJobWorker>();

        return scheduler;
    }

    static void AddBasic(this IObjectContainer services)
    {
        // 注册依赖项
        services.TryAddSingleton(XTrace.Log);
        services.TryAddSingleton(DefaultTracer.Instance ??= new DefaultTracer());

        if (!services.Services.Any(e => e.ServiceType == typeof(IConfigProvider)))
            services.TryAddSingleton<IConfigProvider>(JsonConfigProvider.LoadAppSettings());
    }
}