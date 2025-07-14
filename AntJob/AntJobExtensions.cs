using NewLife;
using NewLife.Configuration;
using NewLife.Log;
using NewLife.Model;

namespace AntJob;

/// <summary>蚂蚁调度依赖注入</summary>
public static class AntJobExtensions
{
    /// <summary>注入AntJob，并AddHandler注册作业类。从配置文件Ant.config或星尘注册中心取得服务端地址</summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static Scheduler AddAntJob(this IObjectContainer services)
    {
        // 实例化调度器
        var scheduler = new Scheduler();

        var set = AntSetting.Current;
        services.TryAddSingleton(set);

        services.AddBasic();
        services.AddSingleton(scheduler);

        services.AddHostedService<AntJobWorker>();

        return scheduler;
    }

    /// <summary>注入AntJob，并AddHandler注册作业类。指定服务端地址和接入应用密钥，未指定时从配置文件Ant.config或星尘注册中心取得</summary>
    /// <param name="services"></param>
    /// <param name="server">服务端地址。多地址逗号分隔</param>
    /// <param name="appId">应用标识。默认为程序集名称</param>
    /// <param name="secret">应用密钥。默认由服务端分配</param>
    /// <returns></returns>
    public static Scheduler AddAntJob(this IObjectContainer services, String server, String appId = null, String secret = null)
    {
        // 实例化调度器
        var scheduler = new Scheduler();

        var set = AntSetting.Current;
        services.TryAddSingleton(set);

        if (!server.IsNullOrEmpty())
        {
            set.Server = server;
            if (!String.IsNullOrEmpty(appId)) set.AppId = appId;
            if (!String.IsNullOrEmpty(secret)) set.Secret = secret;
        }

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