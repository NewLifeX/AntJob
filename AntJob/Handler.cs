using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Reflection;

namespace AntJob;

/// <summary>处理器基类，每个作业一个处理器</summary>
/// <remarks>
/// 文档：https://newlifex.com/blood/antjob
/// 
/// 每个作业一个处理器类，负责一个业务处理模块。
/// 例如在数据同步或数据清洗中，每张表就写一个处理器，如果一组数据表有共同特性，还可以为它们封装一个自己的处理器基类。
/// 
/// 定时调度只要当前时间达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑。
/// 
/// 调度器控制方法：Init|Start|Stop|Acquire
/// 任务处理流程：Process->OnProcess->Execute->OnFinish
/// 任务控制方法：Produce|Delay
/// </remarks>
public abstract class Handler : IExtend, ITracerFeature, ILogFeature
{
    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>调度器</summary>
    public Scheduler Schedule { get; set; }

    /// <summary>作业提供者</summary>
    public IJobProvider Provider { get; set; }

    /// <summary>作业模型。启动前作为创建作业的默认值，启动后表示作业当前设置和状态</summary>
    public IJob Job { get; set; }

    /// <summary>是否工作中</summary>
    public Boolean Active { get; private set; }

    /// <summary>调度模式</summary>
    public virtual JobModes Mode { get; set; } = JobModes.Time;

    private volatile Int32 _Busy;
    /// <summary>正在处理中的任务数</summary>
    public Int32 Busy => _Busy;

    /// <summary>处理速度。调度器可根据处理速度来调节</summary>
    protected Int32 Speed { get; set; }

    private Boolean? _supportsAsync;
    /// <summary>是否支持异步执行（自动检测）</summary>
    public Boolean SupportsAsync
    {
        get
        {
            _supportsAsync ??= IsExecuteAsyncOverridden();
            return _supportsAsync.Value;
        }
    }

    /// <summary>最大不活跃时间（秒）。单个任务从开始执行超过该时间仍未结束，视为僵死。默认0表示禁用检测</summary>
    public Int32 MaxInactiveTime { get; set; }
    #endregion

    #region 索引器
    private readonly Dictionary<String, Object> _Items = [];
    /// <summary>扩展数据</summary>
    IDictionary<String, Object> IExtend.Items => _Items;

    /// <summary>用户数据</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Object this[String item] { get => _Items.TryGetValue(item, out var obj) ? obj : null; set => _Items[item] = value; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public Handler()
    {
        Name = GetType().Name.TrimEnd(nameof(Handler));

        // 默认今天
        var now = DateTime.Now;
        var job = new JobModel
        {
            DataTime = now.Date,
            Step = 30,
            Offset = 15,
            Mode = JobModes.Time,
            Cron = "0/30 * * *",
            MaxTask = 1,
        };

        Job = job;
    }

    /// <summary>检测是否重写了ExecuteAsync方法</summary>
    private Boolean IsExecuteAsyncOverridden()
    {
        var currentType = GetType();
        var baseType = typeof(Handler);
        var baseType2 = "AntJob.Extensions.DataHandler".GetTypeEx();

        // 获取当前类型的所有方法，目标方法任意之一被重写即可
        var names = new[] { "ProcessAsync", "OnProcessAsync", "ExecuteAsync", "ProcessItemAsync" };
        //var types = new[] { "Handler", "MessageHandler", "CSharpHandler", "DataHandler", "SqlHandler" };
        var methods = currentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            if (!names.Contains(method.Name)) continue;

            // 如果当前方法的声明类型不是Handler基类，说明被重写了
            var dtype = method.DeclaringType;
            if (dtype != baseType && dtype != baseType2 && /*!types.Contains(dtype.Name) &&*/
                dtype.Assembly != baseType.Assembly && dtype.Assembly != baseType2?.Assembly)
                return true;
        }

        return false;
    }
    #endregion

    #region 基本方法
    /// <summary>初始化</summary>
    /// <remarks>作业处理器启动之前，这里设置Job作业属性后，将会提交给调度平台</remarks>
    public virtual void Init()
    {
        var job = Job;

        // 定时任务默认最大任务数为1
        if (job.Mode == JobModes.Time)
            job.MaxTask = 1;
        else
        {
            // 默认并发数为核心数
            job.MaxTask = Environment.ProcessorCount;
            if (job.MaxTask < 8) job.MaxTask = 8;
        }
    }

    /// <summary>开始工作</summary>
    /// <remarks>调度器通知处理器开始工作，处理器可以做最后的检查，然后进入工作状态</remarks>
    public virtual Boolean Start()
    {
        if (Active) return false;

        var msg = "开始工作";
        var job = Job;
        if (job != null) msg += $" {job.Enable} 区间（{job.DataTime.ToFullString("")}, {job.End.ToFullString("")}） Offset={job.Offset} Step={job.Step} MaxTask={job.MaxTask}";

        using var span = Tracer?.NewSpan($"job:{Name}:Start", msg);
        WriteLog(msg);

        Active = true;

        return true;
    }

    /// <summary>停止</summary>
    public virtual Boolean Stop(String reason)
    {
        if (!Active) return false;

        using var span = Tracer?.NewSpan($"job:{Name}:Stop", reason);
        WriteLog("停止工作 {0}", reason);

        Active = false;

        // 等待正在执行的任务结束，最多15秒。用于配合僵死检测安全退出
        if (_Busy > 0)
        {
            var end = DateTime.UtcNow.AddSeconds(15);
            while (_Busy > 0 && DateTime.UtcNow < end)
            {
                Thread.Sleep(100);
            }
            if (_Busy > 0) WriteLog("仍有{0}个任务未结束（可能僵死），放弃等待", _Busy);
        }

        return true;
    }
    #endregion

    #region 申请任务
    /// <summary>申请任务</summary>
    /// <remarks>
    /// 业务应用根据使用场景，可重载Acquire并返回空来阻止创建新任务
    /// </remarks>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public virtual async Task<ITask[]> Acquire(Int32 count)
    {
        var prv = Provider;
        var job = Job;

        using var span = Tracer?.NewSpan($"job:{Name}:Acquire", new { count, maxTask = job.MaxTask, Busy });

        // 循环申请任务，喂饱处理器
        var rs = await prv.Acquire(job, null, count);
        if (span != null) span.Value = rs?.Length ?? 0;

        return rs;
    }
    #endregion

    #region 整体调度
    // 正在执行任务开始时间跟踪，用于僵死检测
    private readonly ConcurrentDictionary<Int32, DateTime> _taskTimes = new();

    /// <summary>准备就绪，增加Busy，避免超额分配</summary>
    /// <param name="task"></param>
    internal void Prepare(ITask task)
    {
        Interlocked.Increment(ref _Busy);
        if (task != null) _taskTimes[task.ID] = DateTime.UtcNow;
    }

    /// <summary>检查处理器是否存活。若启用最大不活跃时间且存在超时未结束任务，则返回false</summary>
    /// <returns>是否仍被视为存活</returns>
    public Boolean CheckAlive()
    {
        if (MaxInactiveTime <= 0) return true;
        if (_Busy == 0) return true;

        var threshold = DateTime.UtcNow.AddSeconds(-MaxInactiveTime);
        foreach (var item in _taskTimes)
        {
            if (item.Value <= threshold)
            {
                WriteLog("检测到任务[{0}]超过最大不活跃时间{1}s，标记作业僵死", item.Key, MaxInactiveTime);
                return false;
            }
        }
        return true;
    }

    /// <summary>处理一项新任务。每个作业任务的顶级函数，由线程池执行，内部调用OnProcess</summary>
    /// <remarks>公开该方法，便于为业务处理器编写单元测试</remarks>
    /// <param name="task"></param>
    public JobContext Process(ITask task)
    {
        if (task == null) return null;

        var result = new TaskResult { ID = task.ID };
        var ctx = new JobContext
        {
            Handler = this,
            Task = task,
            Result = result,
        };

        // APM埋点
        using var span = Schedule?.Tracer?.NewSpan($"job:{Name}", task.Data ?? $"({task.DataTime.ToFullString()}, {task.End.ToFullString()})");
        result.TraceId = span?.TraceId;

        // 较慢的作业，及时报告进度
        if (Speed < 10) Report(ctx, JobStatus.处理中);

        var sw = Stopwatch.StartNew();
        ctx.Stopwatch = sw;
        try
        {
            OnProcess(ctx);

            if (span != null) span.Value = ctx.Total;
        }
        catch (Exception ex)
        {
            ctx.Error = ex;
            span?.SetError(ex, task);

            XTrace.WriteException(ex);
        }
        finally
        {
            Interlocked.Decrement(ref _Busy);
            if (task != null) _taskTimes.TryRemove(task.ID, out _);
        }

        sw.Stop();
        ctx.Cost = sw.Elapsed.TotalMilliseconds;
        Speed = ctx.Speed;

        OnFinish(ctx);
        Schedule?.OnFinish(ctx);

        return ctx;
    }

    /// <summary>处理任务。内部分批调用Excute处理数据，由Process执行</summary>
    /// <remarks>仅用于框架内部使用，用户不应该重载该方法，推荐使用Execute</remarks>
    /// <param name="ctx">作业上下文</param>
    protected virtual void OnProcess(JobContext ctx)
    {
        ctx.Total = 1;
        ctx.Success = Execute(ctx);
    }

    /// <summary>报告任务状态</summary>
    /// <param name="ctx">作业上下文</param>
    /// <param name="status"></param>
    protected virtual void Report(JobContext ctx, JobStatus status)
    {
        ctx.Status = status;
        ctx.Cost = ctx.Stopwatch?.Elapsed.TotalMilliseconds ?? 0;
        Provider?.Report(ctx);
    }

    /// <summary>整个任务完成</summary>
    /// <param name="ctx">作业上下文</param>
    protected virtual void OnFinish(JobContext ctx) => Provider?.Finish(ctx).Wait();
    #endregion

    #region 异步调度
    /// <summary>异步处理一项新任务。每个作业任务的顶级函数，由线程池执行，内部调用OnProcessAsync</summary>
    /// <remarks>公开该方法，便于为业务处理器编写单元测试</remarks>
    /// <param name="task"></param>
    public async Task<JobContext> ProcessAsync(ITask task)
    {
        if (task == null) return null;

        var result = new TaskResult { ID = task.ID };
        var ctx = new JobContext
        {
            Handler = this,
            Task = task,
            Result = result,
        };

        // APM埋点
        using var span = Schedule?.Tracer?.NewSpan($"job:{Name}", task.Data ?? $"({task.DataTime.ToFullString()}, {task.End.ToFullString()})");
        result.TraceId = span?.TraceId;

        // 较慢的作业，及时报告进度
        if (Speed < 10) await ReportAsync(ctx, JobStatus.处理中);

        var sw = Stopwatch.StartNew();
        ctx.Stopwatch = sw;
        try
        {
            await OnProcessAsync(ctx);

            if (span != null) span.Value = ctx.Total;
        }
        catch (Exception ex)
        {
            ctx.Error = ex;
            span?.SetError(ex, task);

            XTrace.WriteException(ex);
        }
        finally
        {
            Interlocked.Decrement(ref _Busy);
            if (task != null) _taskTimes.TryRemove(task.ID, out _);
        }

        sw.Stop();
        ctx.Cost = sw.Elapsed.TotalMilliseconds;
        Speed = ctx.Speed;

        await OnFinishAsync(ctx);
        Schedule?.OnFinish(ctx);

        return ctx;
    }

    /// <summary>异步处理任务。内部分批调用ExecuteAsync处理数据，由ProcessAsync执行</summary>
    /// <remarks>仅用于框架内部使用，用户不应该重载该方法，推荐使用ExecuteAsync</remarks>
    /// <param name="ctx">作业上下文</param>
    protected virtual async Task OnProcessAsync(JobContext ctx)
    {
        ctx.Total = 1;
        ctx.Success = await ExecuteAsync(ctx).ConfigureAwait(false);
    }

    /// <summary>异步报告任务状态</summary>
    /// <param name="ctx">作业上下文</param>
    /// <param name="status"></param>
    protected virtual async Task ReportAsync(JobContext ctx, JobStatus status)
    {
        ctx.Status = status;
        ctx.Cost = ctx.Stopwatch?.Elapsed.TotalMilliseconds ?? 0;
        if (Provider != null) await Provider.Report(ctx).ConfigureAwait(false);
    }

    /// <summary>异步完成整个任务</summary>
    /// <param name="ctx">作业上下文</param>
    protected virtual async Task OnFinishAsync(JobContext ctx)
    {
        if (Provider != null) await Provider.Finish(ctx).ConfigureAwait(false);
    }
    #endregion

    #region 数据处理
    /// <summary>处理一批数据，一个任务内多次调用</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    public virtual Int32 Execute(JobContext ctx) => 0;

    /// <summary>异步处理一批数据，一个任务内多次调用</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    public virtual async Task<Int32> ExecuteAsync(JobContext ctx)
    {
        // 默认实现：将同步Execute包装为异步
        return await Task.Run(() => Execute(ctx)).ConfigureAwait(false);
    }

    /// <summary>生产消息</summary>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public virtual Task<Int32> Produce(String topic, String[] messages, MessageOption option = null) => Provider.Produce(Job?.Name, topic, messages, option);

    /// <summary>跨应用生产消息</summary>
    /// <param name="appId">发布消息到目标应用。留空发布当前应用</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public virtual Task<Int32> Produce(String appId, String topic, String[] messages, MessageOption option = null)
    {
        option ??= new();
        option.AppId = appId;
        return Provider.Produce(Job?.Name, topic, messages, option);
    }

    /// <summary>延迟执行，指定下一次执行时间</summary>
    /// <param name="ctx">作业上下文</param>
    /// <param name="nextTime"></param>
    public virtual void Delay(JobContext ctx, DateTime nextTime)
    {
        ctx.Status = JobStatus.延迟;
        ctx.NextTime = nextTime;
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
    public void WriteLog(String format, params Object[] args) => Log?.Info(Name + " " + format, args);
    #endregion
}