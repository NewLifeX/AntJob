using System.Diagnostics;
using System.Net;
using AntJob.Data.Entity;
using AntJob.Server.Services;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Model;
using NewLife.Remoting;
using NewLife.Threading;
using Stardust.Registry;
using XCode;

namespace AntJob.Server;

/// <summary>蚂蚁调度中心后台服务。负责启动ApiServer、定时清理、星尘注册</summary>
/// <remarks>
/// 被 Program.cs 注册为 HostedService，负责：
/// 1. 初始化数据库（InitData）
/// 2. 启动 ApiServer（TCP 9999），注册 AntService
/// 3. 定时清理过期在线和任务数据
/// 4. 向星尘注册中心注册服务
/// </remarks>
public class Worker : IHostedService
{
    private readonly IRegistry _registry;
    private readonly ICacheProvider _cacheProvider;
    private readonly IServiceProvider _provider;
    private readonly AntJobSetting _setting;
    private readonly ITracer _tracer;
    private ApiServer _server;

    /// <summary>实例化后台服务</summary>
    /// <param name="cacheProvider">缓存提供者，用于分布式锁</param>
    /// <param name="provider">服务提供者</param>
    /// <param name="setting">蚂蚁配置</param>
    /// <param name="tracer">性能跟踪器</param>
    public Worker(ICacheProvider cacheProvider, IServiceProvider provider, AntJobSetting setting, ITracer tracer)
    {
        _cacheProvider = cacheProvider;
        _provider = provider;
        _setting = setting;
        _tracer = tracer;
        _registry = provider.GetService<IRegistry>();
    }

    /// <summary>异步启动。初始化数据、启动 RPC 服务、定时清理、注册到星尘</summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitData();

        //var set = AntJobSetting.Current;
        var set = _setting;

        // 实例化RPC服务端，指定端口，指定ServiceProvider，用于依赖注入获取接口服务层
        var server = new ApiServer(set.Port)
        {
            ServiceProvider = _provider,
            ShowError = true,

            Tracer = _tracer,
            Log = XTrace.Log,
        };

        server.Register<AntService>();

        // 本地结点
        AntService.Local = new IPEndPoint(NetHelper.MyIP(), set.Port);

        server.Start();
        _server = server;

        _clearOnlineTimer = new TimerX(ClearOnline, null, 1000, 10 * 1000) { Async = true };
        _clearItemTimer = new TimerX(ClearItems, null, 10_000, 3600_000) { Async = true };

        // 启用星尘注册中心，向注册中心注册服务，服务消费者将自动更新服务端地址列表
        _registry?.RegisterAsync("AntServer", $"tcp://*:{server.Port}");
    }

    /// <summary>异步停止。释放定时器资源</summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _clearOnlineTimer.TryDispose();
        _clearItemTimer.TryDispose();

        // 释放监听端口，避免进程重启或同进程再次启动时端口占用
        _server.TryDispose();
        _server = null;

        return Task.CompletedTask;
    }

    private static async Task InitData()
    {
        var set = NewLife.Setting.Current;
        if (set.IsNew)
        {
            set.DataPath = @"..\Data";

            set.Save();
        }

        var set2 = XCodeSetting.Current;
        if (set2.IsNew)
        {
            set2.Debug = true;
            set2.ShowSQL = false;
            set2.TraceSQLTime = 3000;
            //set2.SQLiteDbPath = @"..\Data";

            set2.Save();
        }

        // 等待反向工程完成，避免服务已开始接收请求但数据表尚未就绪
        await EntityFactory.InitAllAsync();
    }

    #region 清理过时
    private TimerX _clearOnlineTimer;

    //每10s清除一次UpdateTime超10分钟未更新的
    private static void ClearOnline(Object state) => AppService.ClearOnline(10);
    #endregion

    #region 清理任务项
    private TimerX _clearItemTimer;

    private static void ClearItems(Object state)
    {
        var sw = Stopwatch.StartNew();
        var rs = JobService.ClearItems();
        if (rs > 0)
        {
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            var speed = rs * 1000 / ms;
            XTrace.WriteLine("共删除作业项[{0:n0}]行，耗时{1:n0}ms，速度{2:n0}tps", rs, ms, speed);
        }
    }
    #endregion
}