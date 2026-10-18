using System;
using System.Net;
using System.Threading.Tasks;
using AntJob.Server;
using AntJob.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using NewLife;
using NewLife.Model;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Security;
using Xunit;

namespace AntTest;

/// <summary>RPC 调度中心测试夹具。以随机端口启动 ApiServer（TCP），共享 SQLite 数据库</summary>
public sealed class RpcServerFixture : IAsyncLifetime
{
    private ApiServer _server;
    private ServiceProvider _provider;

    /// <summary>调度中心地址，如 tcp://127.0.0.1:5000</summary>
    public String Server { get; private set; }

    /// <summary>服务端配置</summary>
    public AntJobSetting Setting { get; private set; }

    /// <summary>应用认证服务</summary>
    public AppService AppService { get; private set; }

    /// <summary>作业调度服务</summary>
    public JobService JobService { get; private set; }

    /// <summary>服务提供者</summary>
    public IServiceProvider Provider => _provider;

    /// <summary>缓存提供者</summary>
    public ICacheProvider CacheProvider { get; private set; }

    public Task InitializeAsync()
    {
        XTrace.UseConsole();

        TestDb.Init();

        Setting = new AntJobSetting
        {
            Port = 0,
            AutoRegistry = true,
            TokenSecret = "HS256:antjob_test_secret",
            Debug = false,
        };

        var log = XTrace.Log;
        CacheProvider = new CacheProvider();
        AppService = new AppService(new SaltPasswordProvider { Algorithm = "md5", SaltTime = 60 }, Setting, log);
        JobService = new JobService(AppService, CacheProvider, DefaultTracer.Instance ??= new DefaultTracer(), log);

        var services = new ServiceCollection();
        services.AddSingleton(Setting);
        services.AddSingleton(AppService);
        services.AddSingleton(JobService);
        services.AddSingleton(CacheProvider);
        services.AddSingleton<ILog>(log);
        var tracer = DefaultTracer.Instance ??= new DefaultTracer();
        services.AddSingleton<ITracer>(tracer);
        _provider = services.BuildServiceProvider();

        // 客户端初始化依赖全局服务提供者，测试期间指向有效实例
        ObjectContainer.Provider = _provider;

        _server = new ApiServer(0)
        {
            ServiceProvider = _provider,
            ShowError = true,
            Log = log,
        };
        _server.Register<AntService>();
        _server.Start();

        AntService.Local = new IPEndPoint(IPAddress.Loopback, _server.Port);
        Server = $"tcp://127.0.0.1:{_server.Port}";

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // 恢复一个可用的全局提供者，避免把已释放实例留给后续测试
        ObjectContainer.Provider = new ObjectContainer().BuildServiceProvider();

        _server.TryDispose();
        _provider.TryDispose();

        return Task.CompletedTask;
    }
}
