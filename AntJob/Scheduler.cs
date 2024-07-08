using AntJob.Data;
using AntJob.Handlers;
using AntJob.Providers;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using Stardust.Registry;

namespace AntJob;

/// <summary>作业调度器</summary>
public class Scheduler : DisposeBase
{
    #region 属性
    /// <summary>处理器集合</summary>
    public List<Handler> Handlers { get; } = [];

    /// <summary>作业提供者</summary>
    public IJobProvider Provider { get; set; }

    /// <summary>服务提供者</summary>
    public IServiceProvider ServiceProvider { get; set; }

    private ICache _cache = MemoryCache.Default;
    #endregion

    #region 构造
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Stop();
    }
    #endregion

    #region 处理器
    /// <summary>添加处理器</summary>
    /// <param name="handler"></param>
    public void AddHandler(Handler handler) => Handlers.Add(handler);

    /// <summary>按类型添加处理器，支持依赖注入</summary>
    /// <typeparam name="T"></typeparam>
    public void AddHandler<T>() where T : Handler
    {
        var services = ObjectContainer.Current;
        var prv = ObjectContainer.Provider;
        services.AddTransient<T>();

        Handlers.Add(prv.GetService<T>());
    }
    #endregion

    #region 核心方法
    /// <summary>加入调度中心，从注册中心获取地址，自动识别RPC/Http</summary>
    /// <param name="set"></param>
    /// <returns></returns>
    public IJobProvider Join(AntSetting set)
    {
        var server = set.Server;

        var registry = ServiceProvider?.GetService<IRegistry>();
        if (registry != null)
        {
            var rs = registry.ResolveAsync("AntServer").Result;
            var svrs = registry.ResolveAddressAsync("AntServer").Result;
            if (svrs != null && svrs.Length > 0) server = svrs.Join();
        }

        if (server.IsNullOrEmpty()) return null;
        set.Server = server;

        // 根据地址决定用Http还是RPC
        //var servers = server.Split(",");
        //if (servers.Any(e => e.StartsWithIgnoreCase("http://", "https://")))
        //{
        //    var http = new HttpJobProvider
        //    {
        //        Debug = debug,
        //        Server = server,
        //        AppId = appId,
        //        Secret = secret,
        //    };

        //    // 如果有注册中心，则使用注册中心的服务发现
        //    if (registry != null)
        //    {
        //        //http.Client = registry.CreateForService("AntServer") as ApiHttpClient;
        //        //http.Client.RoundRobin = false;
        //    }

        //    Provider = http;
        //}
        //else
        {
            var rpc = new NetworkJobProvider(set);

            Provider = rpc;
        }

        return Provider;
    }

    /// <summary>开始</summary>
    public void Start()
    {
        OnStart();
    }

    /// <summary>异步开始。使用定时器尝试连接服务端</summary>
    public void StartAsync()
    {
        _timerStart = new TimerX(CheckStart, null, 100, 15000, "Job") { Async = true };
    }

    private Boolean _inited;
    private TimerX _timerStart;
    private void CheckStart(Object state)
    {
        if (!_inited)
        {
            try
            {
                OnStart();

                _inited = true;
            }
            catch (Exception ex)
            {
                Log?.Error(ex.Message);
            }
        }

        if (_inited) _timerStart.TryDispose();
    }

    private void OnStart()
    {
        // 检查本地添加的处理器
        var hs = Handlers;
        if (hs.Count == 0) throw new ArgumentNullException(nameof(Handlers), "没有可用处理器");

        // 埋点
        Tracer ??= ServiceProvider?.GetService<ITracer>();
        using var span = Tracer?.NewSpan("job:SchedulerStart");

        // 启动作业提供者
        var prv = Provider;
        prv ??= Provider = new FileJobProvider();
        prv.Schedule ??= this;

        if (prv is ITracerFeature tf) tf.Tracer = Tracer;
        if (prv is ILogFeature lf) lf.Log = Log;

        prv.Start();

        // 获取本应用在调度中心管理的所有作业
        var jobs = prv.GetJobs() ?? [];
        //if (jobs == null || jobs.Length == 0) throw new Exception("调度中心没有可用作业");

        // 输出日志
        WriteLog($"启动任务调度引擎[{prv}]，作业[{hs.Count}]项，定时{Period}秒");

        // 设置日志
        foreach (var handler in hs)
        {
            handler.Schedule = this;
            handler.Provider = prv;

            // 查找作业参数，分配给处理器
            var job = jobs.FirstOrDefault(e => e.Name == handler.Name);
            if (job == null || !job.Enable) continue;

            if (job != null && job.Mode == 0) job.Mode = handler.Mode;
            handler.Job = job;

            handler.Tracer = Tracer;
            handler.Log = Log;

            try
            {
                handler.Start();
            }
            catch (Exception ex)
            {
                Log?.Error("作业[{0}]启动失败！{1}", handler.GetType().FullName, ex.Message);
            }
        }

        // 定时执行
        if (Period > 0) _timer = new TimerX(Loop, null, 100, Period * 1000, "Job") { Async = true };
    }

    /// <summary>停止</summary>
    public void Stop()
    {
        using var span = Tracer?.NewSpan("job:SchedulerStop");

        _timer.TryDispose();
        _timer = null;

        Provider?.Stop();

        foreach (var handler in Handlers)
        {
            try
            {
                handler.Stop("SchedulerStop");
            }
            catch (Exception ex)
            {
                Log?.Error("作业[{0}]停止失败！{1}", handler.GetType().FullName, ex.Message);
            }
        }
    }

    /// <summary>任务调度</summary>
    /// <returns></returns>
    public Boolean Process()
    {
        var prv = Provider;

        // 查询所有处理器
        var hs = Handlers;

        // 拿到处理器对应的作业
        var jobs = prv.GetJobs();
        if (jobs == null) return false;

        // 运行时动态往集合里面加处理器，为了配合Sql+C#
        CheckHandlers(prv, jobs, hs);

        var flag = false;
        // 遍历处理器，给空闲的增加任务
        foreach (var handler in hs)
        {
            var job = jobs.FirstOrDefault(e => e.Name == handler.Name);
            // 找不到或者已停用
            if (job == null || !job.Enable)
            {
                if (handler.Active) handler.Stop("ConfigChanged");
                continue;
            }

            // 可能外部添加的Worker并不完整
            handler.Schedule = this;
            handler.Provider = prv;

            // 更新作业参数，并启动处理器
            handler.Job = job;
            if (job.Mode == 0) job.Mode = handler.Mode;
            if (!handler.Active)
            {
                try
                {
                    handler.Start();
                }
                catch (Exception ex)
                {
                    Log?.Error("作业[{0}]启动失败！{1}", handler.GetType().FullName, ex.Message);
                }
            }

            // 如果正在处理任务数没达到最大并行度，则继续安排任务
            var max = job.MaxTask;
            if (prv is NetworkJobProvider nprv)
            {
                // 如果是网络提供者，则根据在线节点数平分并行度
                var ps = nprv.Peers;
                if (ps != null && ps.Length > 0)
                {
                    max = max < ps.Length ? 1 : (Int32)Math.Round((Double)max / ps.Length);
                }
            }
            var count = max - handler.Busy;
            if (count > 0)
            {
                // 循环申请任务，喂饱处理器
                var ts = handler.Acquire(count);

                // 送给处理器处理
                for (var i = 0; i < count && ts != null && i < ts.Length; i++)
                {
                    // 准备就绪，增加Busy，避免超额分配
                    handler.Prepare(ts[i]);

                    // 使用线程池调度，避免Task排队影响使用
                    ThreadPool.QueueUserWorkItem(s => handler.Process(s as ITask), ts[i]);
                }

                if (ts != null && ts.Length > 0) flag = true;
            }
        }

        return flag;
    }

    private void CheckHandlers(IJobProvider provider, IList<IJob> jobs, IList<Handler> handlers)
    {
        foreach (var job in jobs)
        {
            var handler = handlers.FirstOrDefault(e => e.Name == job.Name);
            if (handler == null && job.Enable && !job.ClassName.IsNullOrEmpty())
            {
                // 遇到废弃作业时，避免反复输出日志
                if (!_cache.Add($"job:NewHandler:{job.Name}", 1, 3600)) return;

                using var span = Tracer?.NewSpan($"job:NewHandler", job);

                WriteLog("发现未知作业[{0}]@[{1}]", job.Name, job.ClassName);
                try
                {
                    // 实例化一个处理器
                    var type = Type.GetType(job.ClassName) ?? (handlers.Where(e => e.GetType().FullName == job.ClassName)?.FirstOrDefault()?.GetType());
                    if (type != null)
                    {
                        handler = type.CreateInstance() as Handler;
                        if (handler != null)
                        {
                            WriteLog("添加新作业[{0}]@[{1}]", job.Name, job.ClassName);

                            handler.Name = job.Name;
                            handler.Schedule = this;
                            handler.Provider = provider;

                            if (handler is MessageHandler messageHandler && !job.Topic.IsNullOrEmpty())
                                messageHandler.Topic = job.Topic;

                            handler.Log = Log;
                            handler.Tracer = Tracer;
                            handler.Start();

                            handlers.Add(handler);
                        }
                    }
                }
                catch (Exception ex)
                {
                    span?.SetError(ex, null);
                    //XTrace.WriteException(ex);
                    Log?.Error("作业[{0}]启动失败！{1}", handler?.GetType().FullName, ex.Message);
                }
            }
        }
    }

    /// <summary>已完成</summary>
    /// <param name="ctx"></param>
    internal protected virtual void OnFinish(JobContext ctx) => _timer?.SetNext(-1);
    #endregion

    #region 定时调度
    /// <summary>定时轮询周期。默认5秒</summary>
    public Int32 Period { get; set; } = 5;

    private TimerX _timer;

    private void Loop(Object state)
    {
        // 任务调度
        var rs = Process();

        // 如果有数据，马上开始下一轮
        if (rs) TimerX.Current.SetNext(-1);
    }
    #endregion

    #region 日志
    /// <summary>性能跟踪器</summary>
    public ITracer Tracer { get; set; }

    /// <summary>日志</summary>
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}