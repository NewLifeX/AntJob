using System.Diagnostics;
using System.Net;
using AntJob.Data.Entity;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Model;
using NewLife.Remoting;
using NewLife.Threading;
using Stardust.Registry;
using XCode;

namespace AntJob.Server;

public class Worker : IHostedService
{
    private readonly IRegistry _registry;
    private readonly ICacheProvider _cacheProvider;
    private readonly IServiceProvider _provider;
    private readonly AntJobSetting _setting;
    private readonly ITracer _tracer;

    public Worker(ICacheProvider cacheProvider, IServiceProvider provider, AntJobSetting setting, ITracer tracer)
    {
        _cacheProvider = cacheProvider;
        _provider = provider;
        _setting = setting;
        _tracer = tracer;
        _registry = provider.GetService<IRegistry>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitData();

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

        //// 数据缓存，也用于全局锁，支持MemoryCache和Redis
        //if (_cacheProvider.Cache is not FullRedis && !set.RedisCache.IsNullOrEmpty())
        //{
        //    var redis = new Redis { Timeout = 5_000 + 1_000 };
        //    redis.Init(set.RedisCache);

        //    _cacheProvider.Cache = redis;
        //}

        server.Start();

        _clearOnlineTimer = new TimerX(ClearOnline, null, 1000, 10 * 1000) { Async = true };
        _clearItemTimer = new TimerX(ClearItems, null, 10_000, 3600_000) { Async = true };

        // 启用星尘注册中心，向注册中心注册服务，服务消费者将自动更新服务端地址列表
        if (_registry != null) await _registry.RegisterAsync("AntServer", $"tcp://*:{server.Port}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _clearOnlineTimer.TryDispose();
        _clearItemTimer.TryDispose();

        return Task.CompletedTask;
    }

    private static void InitData()
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

        _ = EntityFactory.InitAllAsync();
    }

    #region 清理过时
    private TimerX _clearOnlineTimer;

    //每10s清除一次UpdateTime超10分钟未更新的
    private static void ClearOnline(Object state)
    {
        var ls = AppOnline.GetOnlines(10);
        foreach (var item in ls)
        {
            item.Delete();
        }
    }
    #endregion

    #region 清理任务项
    private TimerX _clearItemTimer;

    private static void ClearItems(Object state)
    {
        // 遍历所有作业
        var p = 0;
        var rs = 0;
        var sw = Stopwatch.StartNew();
        while (true)
        {
            var list = Job.FindAll(null, null, null, p, 1000);
            if (list.Count == 0) break;

            foreach (var job in list)
            {
                try
                {
                    rs += job.DeleteItems();
                }
                catch (Exception ex)
                {
                    XTrace.WriteException(ex);
                }
            }

            if (list.Count < 1000) break;
            p += list.Count;
        }

        // 删除作业已不存在的任务
        rs += JobTask.DeleteNoJob();

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