using AntJob.Data;
using NewLife;
using NewLife.Log;
#if !NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace AntJob.Providers;

/// <summary>作业提供者接口</summary>
public interface IJobProvider
{
    /// <summary>调度器</summary>
    Scheduler Schedule { get; set; }

    /// <summary>开始工作</summary>
    Task Start();

    /// <summary>停止工作</summary>
    Task Stop();

    /// <summary>获取所有作业。调度器定期获取以更新作业参数</summary>
    /// <returns></returns>
    Task<IJob[]> GetJobs();

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    Task<IJob> SetJob(IJob job);

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    Task<ITask[]> Acquire(IJob job, String topic, Int32 count);

    /// <summary>生产消息</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    Task<Int32> Produce(String job, String topic, String[] messages, MessageOption option);

    /// <summary>报告进度</summary>
    /// <param name="ctx">上下文</param>
    Task Report(JobContext ctx);

    /// <summary>完成任务</summary>
    /// <param name="ctx">上下文</param>
    Task Finish(JobContext ctx);
}

/// <summary>任务提供者基类</summary>
public abstract class JobProvider : DisposeBase, IJobProvider, ITracerFeature, ILogFeature
{
    /// <summary>调度器</summary>
    public Scheduler Schedule { get; set; }

    /// <summary>开始工作</summary>
    public virtual Task Start() => TaskEx.CompletedTask;

    /// <summary>停止工作</summary>
    public virtual Task Stop() => TaskEx.CompletedTask;

    /// <summary>获取所有作业名称</summary>
    /// <returns></returns>
    public abstract Task<IJob[]> GetJobs();

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public abstract Task<IJob> SetJob(IJob job);

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public abstract Task<ITask[]> Acquire(IJob job, String topic, Int32 count);

    /// <summary>生产消息</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public virtual Task<Int32> Produce(String job, String topic, String[] messages, MessageOption option = null) => Task.FromResult(0);

    /// <summary>报告进度，每个任务多次调用</summary>
    /// <param name="ctx">上下文</param>
    public virtual Task Report(JobContext ctx) => TaskEx.CompletedTask;

    /// <summary>完成任务，每个任务只调用一次</summary>
    /// <param name="ctx">上下文</param>
    public virtual Task Finish(JobContext ctx) => TaskEx.CompletedTask;

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