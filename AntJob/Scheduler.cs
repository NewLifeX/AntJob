using AntJob.Data;
using AntJob.Handlers;
using AntJob.Providers;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Remoting.Clients;
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

    /// <summary>配置信息</summary>
    public AntSetting Setting { get; set; }

    private readonly ICache _cache = MemoryCache.Default;
    private readonly Object _handlersLock = new();
    private readonly SemaphoreSlim _acquireSemaphore = new(8);
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
    public Scheduler AddHandler(Handler handler)
    {
        lock (_handlersLock)
        {
            // 启动失败重试时会再次从容器解析处理器，按类型和名称去重，避免同一个作业被重复执行
            if (!Handlers.Any(e => e.GetType() == handler.GetType() && e.Name.EqualIgnoreCase(handler.Name))) Handlers.Add(handler);
        }

        return this;
    }

    /// <summary>获取处理器快照。避免定时线程与下行通知线程并发枚举时集合被修改</summary>
    /// <returns></returns>
    internal Handler[] GetHandlers()
    {
        lock (_handlersLock)
        {
            return Handlers.ToArray();
        }
    }

    /// <summary>按类型添加处理器，支持依赖注入</summary>
    /// <typeparam name="T"></typeparam>
    public Scheduler AddHandler<T>() where T : Handler
    {
        // 把服务类型注册到容器中，以便后续获取
        var ioc = ServiceProvider?.GetService<IObjectContainer>() ?? ObjectContainer.Current;
        ioc.AddTransient<Handler, T>();

        return this;
    }
    #endregion

    #region 核心方法
    /// <summary>加入调度中心，从注册中心获取地址，自动识别RPC/Http</summary>
    /// <param name="set"></param>
    /// <returns></returns>
    [Obsolete("=>JoinAsync")]
    public IJobProvider Join(AntSetting set)
    {
        JoinAsync(set).Wait();

        return Provider;
    }

    /// <summary>加入调度中心，从注册中心获取地址，自动识别RPC/Http</summary>
    /// <param name="set"></param>
    /// <returns></returns>
    public async Task JoinAsync(AntSetting set)
    {
        var server = set.Server;
        var isHttp = server.StartsWithIgnoreCase("http://", "https://");

        var registry = set.UseRegistry ? ServiceProvider?.GetService<IRegistry>() : null;
        if (registry != null)
        {
            // 按地址协议决定服务名优先级；未显式配置地址时优先 HTTP 通道（AntWeb）
            var names = server.IsNullOrEmpty() || isHttp
                ? new[] { "AntWeb", "AntServer" }
                : new[] { "AntServer", "AntWeb" };

            foreach (var name in names)
            {
                var svrs = await registry.ResolveAddressAsync(name);
                if (svrs != null && svrs.Length > 0)
                {
                    server = svrs.Join();
                    break;
                }
            }
        }

        if (server.IsNullOrEmpty()) return;
        set.Server = server;

        isHttp = server.StartsWithIgnoreCase("http://", "https://");
        if (isHttp)
            Provider = new HttpJobProvider(set);
        else
            Provider = new NetworkJobProvider(set);
    }

    /// <summary>开始调度。推荐使用StartAsync</summary>
    [Obsolete("=>StartAsync")]
    public void Start()
    {
        if (Setting != null) Join(Setting);

        OnStart().Wait();
    }

    /// <summary>异步开始。使用定时器尝试连接服务端</summary>
    public void StartAsync()
    {
        _timerStart = new TimerX(CheckStart, null, 100, 15000, "Job") { Async = true };
    }

    private Boolean _inited;
    private TimerX _timerStart;
    private async Task CheckStart(Object state)
    {
        if (!_inited)
        {
            try
            {
                if (Setting != null) await JoinAsync(Setting);

                await OnStart();

                _inited = true;
            }
            catch (Exception ex)
            {
                Log?.Error(ex.Message);

                // 启动失败时释放本次创建的提供者，避免定时重试过程中不断累积连接与定时器
                Provider.TryDispose();
                Provider = null;
            }
        }

        if (_inited) _timerStart.TryDispose();
    }

    private async Task OnStart()
    {
        // 从容器中获取所有服务
        var serviceProvider = ServiceProvider;
        foreach (var item in serviceProvider.GetServices<Handler>())
        {
            AddHandler(item);
        }
        // 有可能DI中注入IObjectContainer，后者再次注册Handler
        var ioc = serviceProvider?.GetService<IObjectContainer>() ?? ObjectContainer.Current;
        if (ioc != null && ioc.GetType().Namespace != serviceProvider?.GetType().Namespace)
        {
            // 内外容器混合，创建新的服务提供者来解析作业处理器。作业处理器所依赖的服务，可能由外部服务提供者提供
            var serviceProvider2 = ioc.BuildServiceProvider(serviceProvider);
            foreach (var item in serviceProvider2.GetServices<Handler>())
            {
                AddHandler(item);
            }
        }

        // 检查本地添加的处理器
        var hs = GetHandlers();
        if (hs.Length == 0) throw new ArgumentNullException(nameof(Handlers), "没有可用处理器");

        // 埋点
        Tracer ??= serviceProvider?.GetService<ITracer>();
        using var span = Tracer?.NewSpan("job:SchedulerStart");

        // 启动作业提供者
        var prv = Provider;
        prv ??= Provider = new FileJobProvider();
        prv.Schedule ??= this;

        if (prv is ITracerFeature tf) tf.Tracer = Tracer;
        if (prv is ILogFeature lf) lf.Log = Log;

        await prv.Start();

        // 获取本应用在调度中心管理的所有作业
        var jobs = await prv.GetJobs();

        // 输出日志
        WriteLog($"启动任务调度引擎[{prv}]，作业[{hs.Length}]项，定时{Period}秒");

        // 设置日志
        foreach (var handler in hs)
        {
            handler.Schedule = this;
            handler.Provider = prv;
            handler.Tracer ??= Tracer;
            handler.Log ??= Log;

            // 查找作业参数，分配给处理器
            var job = jobs?.FirstOrDefault(e => e.Name == handler.Name);
            if (job == null || !job.Enable) continue;

            if (job != null && job.Mode == 0) job.Mode = handler.Mode;
            handler.Job = job;

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

        foreach (var handler in GetHandlers())
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

        Provider?.Stop().Wait();
    }

    /// <summary>申请任务。限制并发度，避免多作业同时请求压垮调度中心</summary>
    /// <param name="handler">处理器</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    private async Task<ITask[]> AcquireTasks(Handler handler, Int32 count)
    {
        await _acquireSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await handler.Acquire(count).ConfigureAwait(false);
        }
        finally
        {
            _acquireSemaphore.Release();
        }
    }

    private Int32 _processing;
    private Int32 _pending;

    /// <summary>任务调度</summary>
    /// <remarks>定时器异步回调，网络往返超过调度周期时可能重入；上一轮未结束时直接跳过，避免重复申请任务</remarks>
    /// <returns></returns>
    public async Task<Boolean> Process()
    {
        if (Interlocked.CompareExchange(ref _processing, 1, 0) != 0)
        {
            // 运行期间到达的唤醒不能丢，当前轮结束后立即补跑
            Interlocked.Exchange(ref _pending, 1);
            return false;
        }

        try
        {
            var rs = await ProcessCore().ConfigureAwait(false);

            // 有待处理轮次时告知调用方马上再来一轮
            if (Interlocked.Exchange(ref _pending, 0) != 0) rs = true;

            return rs;
        }
        finally
        {
            Interlocked.Exchange(ref _processing, 0);
        }
    }

    private async Task<Boolean> ProcessCore()
    {
        var prv = Provider;

        // 拿到处理器对应的作业
        var jobs = await prv.GetJobs();
        if (jobs == null) return false;

        // 运行时动态往集合里面加处理器，为了配合Sql+C#
        CheckHandlers(prv, jobs);

        // 作业按名称建索引，避免每个处理器都线性扫描
        var jobMap = new Dictionary<String, IJob>(StringComparer.OrdinalIgnoreCase);
        foreach (var job in jobs)
        {
            if (!job.Name.IsNullOrEmpty()) jobMap[job.Name] = job;
        }

        var flag = false;
        Handler inactive = null;
        var acquires = new List<(Handler Handler, IJob Job, Int32 Count, Task<ITask[]> Task)>();
        // 遍历处理器快照，给空闲的增加任务
        foreach (var handler in GetHandlers())
        {
            var job = !handler.Name.IsNullOrEmpty() && jobMap.TryGetValue(handler.Name, out var jb) ? jb : null;
            // 找不到或者已停用
            if (job == null || !job.Enable)
            {
                if (handler.Active) handler.Stop("ConfigChanged");
                continue;
            }

            // 可能外部添加的Worker并不完整
            handler.Schedule = this;
            handler.Provider = prv;

            // 僵死检测：如果处理器不再存活，立即停止调度并退出进程
            if (!handler.CheckAlive())
            {
                inactive = handler;
                continue;
            }

            // 更新作业参数，并启动处理器
            handler.Job = job;
            if (job.Mode == 0) job.Mode = handler.Mode;
            if (job.MaxTime > 0) handler.MaxInactiveTime = job.MaxTime * 2;

            // 服务端修改主题后，同步给消息处理器
            if (handler is MessageHandler mh && !job.Topic.IsNullOrEmpty()) mh.Topic = job.Topic;
            if (!handler.Active)
            {
                try
                {
                    handler.Tracer ??= Tracer;
                    handler.Log ??= Log;
                    handler.Start();
                }
                catch (Exception ex)
                {
                    Log?.Error("作业[{0}]启动失败！{1}", handler.GetType().FullName, ex.Message);
                }
            }

            // 空闲退避：消息型作业在未收到通知前不必每轮都申请
            if (handler.NextAcquire > TimerX.Now) continue;

            // 如果正在处理任务数没达到最大并行度，则继续安排任务
            //var max = job.MaxTask;
            //if (prv is NetworkJobProvider nprv)
            //{
            //    // 如果是网络提供者，则根据在线节点数平分并行度
            //    var ps = nprv.Peers;
            //    if (ps != null && ps.Length > 0)
            //    {
            //        max = max < ps.Length ? 1 : (Int32)Math.Round((Double)max / ps.Length);
            //    }
            //}
            //var count = max - handler.Busy;
            var count = job.MaxTask - handler.Busy;
            if (count > 0) acquires.Add((handler, job, count, AcquireTasks(handler, count)));
        }

        // 并行申请任务，缩短多作业场景的轮次耗时；并发度受限，避免瞬时压垮调度中心
        if (acquires.Count > 0)
        {
            try
            {
                await Task.WhenAll(acquires.Select(e => e.Task)).ConfigureAwait(false);
            }
            catch
            {
                // 单个作业申请失败在下面逐个处理
            }

            foreach (var item in acquires)
            {
                var handler = item.Handler;
                var job = item.Job;
                var ts = item.Task.Status == TaskStatus.RanToCompletion ? item.Task.Result : null;
                if (item.Task.IsFaulted)
                    Log?.Error("作业[{0}]申请任务失败！{1}", handler.Name, item.Task.Exception?.GetTrue()?.Message);

                // 消息型作业在没有任务时进入空闲退避，收到通知后重置
                if (ts == null || ts.Length == 0)
                {
                    var idle = (prv as NetworkJobProvider)?.MessageIdlePeriod ?? 0;
                    if (idle > 0 && job.Mode == JobModes.Message) handler.NextAcquire = TimerX.Now.AddSeconds(idle);
                }
                else
                    handler.NextAcquire = DateTime.MinValue;

                if (ts == null || ts.Length == 0) continue;

                // 送给处理器处理
                for (var i = 0; i < item.Count && i < ts.Length; i++)
                {
                    // 准备就绪，增加Busy，避免超额分配
                    handler.Prepare(ts[i]);

                    // 自动检测并选择同步或异步调用路径
                    if (handler.SupportsAsync)
                    {
                        // 回调函数内不能用ts[i]，因为i会变
                        ThreadPool.QueueUserWorkItem(async s =>
                        {
                            try
                            {
                                await handler.ProcessAsync(s as ITask).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Log?.Error("异步作业[{0}]执行失败！{1}", handler.GetType().FullName, ex.Message);
                            }
                        }, ts[i]);
                    }
                    else
                    {
                        // 使用线程池调度，避免Task排队影响使用
                        ThreadPool.QueueUserWorkItem(s => handler.Process(s as ITask), ts[i]);
                    }
                }

                flag = true;
            }
        }

        // 如果存在僵尸处理器，立即停止调度并退出进程
        if (inactive != null)
        {
            using var span = Tracer?.NewSpan("job:HandlerInactive", inactive.Name);

            WriteLog("检测到作业[{0}]出现僵死，停止调度并退出进程", inactive.Name);
            var ev = ServiceProvider?.GetService<IEventProvider>();
            ev?.WriteErrorEvent("AntJob", $"检测到作业[{inactive.Name}]出现僵死，停止调度并退出进程");
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                span?.SetError(ex, null);
                Log?.Error("调度器停止失败！{0}", ex.Message);
            }
            finally
            {
                Environment.Exit(250);
            }
        }

        return flag;
    }

    private void CheckHandlers(IJobProvider provider, IList<IJob> jobs)
    {
        var handlers = GetHandlers();
        var handlerMap = new Dictionary<String, Handler>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in handlers)
        {
            if (!item.Name.IsNullOrEmpty()) handlerMap[item.Name] = item;
        }

        foreach (var job in jobs)
        {
            var handler = !job.Name.IsNullOrEmpty() && handlerMap.TryGetValue(job.Name, out var hd) ? hd : null;
            if (handler == null && job.Enable && !job.ClassName.IsNullOrEmpty())
            {
                // 遇到废弃作业时，避免反复输出日志
                if (!_cache.Add($"job:NewHandler:{job.Name}", 1, 3600)) continue;

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

                            handler.Log ??= Log;
                            handler.Tracer ??= Tracer;
                            handler.Init();
                            handler.Start();

                            AddHandler(handler);
                            handlerMap[handler.Name] = handler;
                        }
                    }
                }
                catch (Exception ex)
                {
                    span?.SetError(ex, null);
                    Log?.Error("作业[{0}]启动失败！{1}", handler?.GetType().FullName, ex.Message);
                }
            }
        }
    }

    /// <summary>已完成</summary>
    /// <param name="ctx">作业上下文</param>
    internal protected virtual void OnFinish(JobContext ctx) => _timer?.SetNext(-1);
    #endregion

    #region 定时调度
    /// <summary>定时轮询周期。默认5秒</summary>
    public Int32 Period { get; set; } = 5;

    private TimerX _timer;

    /// <summary>唤醒调度。收到服务端通知或需要立即处理时调用</summary>
    public void WakeUp() => _timer?.SetNext(-1);

    private async Task Loop(Object state)
    {
        // 任务调度
        var rs = await Process();

        // 如果有数据，马上开始下一轮
        if (rs) TimerX.Current.SetNext(-1);
    }
    #endregion

    #region 日志
    /// <summary>性能跟踪器</summary>
    public ITracer Tracer { get; set; }

    /// <summary>日志</summary>
    public ILog Log { get; set; }

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}