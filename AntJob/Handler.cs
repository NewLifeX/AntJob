using System.Diagnostics;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using NewLife.Data;
using NewLife.Log;

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
/// 调度器控制方法：Start|Stop|Acquire
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

    private Int32 _speed;
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

        // 默认本月1号
        var now = DateTime.Now;
        var job = new JobModel
        {
            DataTime = now.Date,
            Step = 30,
            Offset = 15,
            Mode = JobModes.Time,
            Cron = "0/30 * * *",
        };

        // 默认并发数为核心数
        job.MaxTask = Environment.ProcessorCount;
        if (job.MaxTask < 8) job.MaxTask = 8;

        Job = job;
    }
    #endregion

    #region 基本方法
    /// <summary>初始化。作业处理器启动之前</summary>
    public virtual void Init() { }

    /// <summary>开始</summary>
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
        WriteLog("停止工作");

        Active = false;

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
    public virtual ITask[] Acquire(Int32 count)
    {
        var prv = Provider;
        var job = Job;

        // 循环申请任务，喂饱处理器
        return prv.Acquire(job, null, count);
    }
    #endregion

    #region 整体调度
    /// <summary>准备就绪，增加Busy，避免超额分配</summary>
    /// <param name="task"></param>
    internal void Prepare(ITask task) => Interlocked.Increment(ref _Busy);

    /// <summary>处理一项新任务</summary>
    /// <param name="task"></param>
    public virtual void Process(ITask task)
    {
        if (task == null) return;

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
        if (_speed < 10) Report(ctx, JobStatus.处理中);

        var sw = Stopwatch.StartNew();
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
            //span?.Dispose();
        }

        sw.Stop();
        ctx.Cost = sw.Elapsed.TotalMilliseconds;
        _speed = ctx.Speed;

        OnFinish(ctx);
        Schedule?.OnFinish(ctx);

        //ctx.Items.Clear();
    }

    /// <summary>处理任务。内部分批处理</summary>
    /// <param name="ctx"></param>
    protected virtual void OnProcess(JobContext ctx)
    {
        ctx.Total = 1;
        ctx.Success = Execute(ctx);
    }

    /// <summary>报告任务状态</summary>
    /// <param name="ctx"></param>
    /// <param name="status"></param>
    protected virtual void Report(JobContext ctx, JobStatus status)
    {
        ctx.Status = status;
        Provider?.Report(ctx);
    }

    /// <summary>整个任务完成</summary>
    /// <param name="ctx"></param>
    protected virtual void OnFinish(JobContext ctx) => Provider?.Finish(ctx);
    #endregion

    #region 数据处理
    /// <summary>处理一批数据，一个任务内多次调用</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    public abstract Int32 Execute(JobContext ctx);

    /// <summary>生产消息</summary>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public virtual Int32 Produce(String topic, String[] messages, MessageOption option = null) => Provider.Produce(Job?.Name, topic, messages, option);

    /// <summary>延迟执行，指定下一次执行时间</summary>
    /// <param name="ctx"></param>
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
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(Name + " " + format, args);
    #endregion
}