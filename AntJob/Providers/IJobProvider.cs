using AntJob.Data;
using NewLife;
using NewLife.Log;

namespace AntJob.Providers;

/// <summary>作业提供者接口</summary>
public interface IJobProvider
{
    /// <summary>调度器</summary>
    Scheduler Schedule { get; set; }

    /// <summary>开始工作</summary>
    void Start();

    /// <summary>停止工作</summary>
    void Stop();

    /// <summary>获取所有作业。调度器定期获取以更新作业参数</summary>
    /// <returns></returns>
    IJob[] GetJobs();

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    IJob SetJob(IJob job);

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    ITask[] Acquire(IJob job, String topic, Int32 count);

    /// <summary>生产消息</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    Int32 Produce(String job, String topic, String[] messages, MessageOption option);

    /// <summary>报告进度</summary>
    /// <param name="ctx">上下文</param>
    void Report(JobContext ctx);

    /// <summary>完成任务</summary>
    /// <param name="ctx">上下文</param>
    void Finish(JobContext ctx);
}

/// <summary>任务提供者基类</summary>
public abstract class JobProvider : DisposeBase, IJobProvider, ITracerFeature, ILogFeature
{
    /// <summary>调度器</summary>
    public Scheduler Schedule { get; set; }

    /// <summary>开始工作</summary>
    public virtual void Start() { }

    /// <summary>停止工作</summary>
    public virtual void Stop() { }

    /// <summary>获取所有作业名称</summary>
    /// <returns></returns>
    public abstract IJob[] GetJobs();

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public abstract IJob SetJob(IJob job);

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public abstract ITask[] Acquire(IJob job, String topic, Int32 count);

    /// <summary>生产消息</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public virtual Int32 Produce(String job, String topic, String[] messages, MessageOption option = null) => 0;

    /// <summary>报告进度，每个任务多次调用</summary>
    /// <param name="ctx">上下文</param>
    public virtual void Report(JobContext ctx) { }

    /// <summary>完成任务，每个任务只调用一次</summary>
    /// <param name="ctx">上下文</param>
    public virtual void Finish(JobContext ctx) { }

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