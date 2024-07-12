using System.Collections;
using AntJob.Data;
using NewLife;
using NewLife.Log;
using NewLife.Serialization;

namespace AntJob.Handlers;

/// <summary>消息调度基类，消费的消息在Data中返回</summary>
public abstract class MessageHandler : Handler
{
    #region 属性
    /// <summary>主题。设置后使用消费调度模式</summary>
    public String Topic { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public MessageHandler()
    {
        Mode = JobModes.Message;

        var job = Job;
        job.BatchSize = 8;
    }
    #endregion

    #region 方法
    /// <summary>参数检查</summary>
    /// <returns></returns>
    public override Boolean Start()
    {
        if (Topic.IsNullOrEmpty()) throw new ArgumentNullException(nameof(Topic), "消息调度要求设置主题");

        return base.Start();
    }

    /// <summary>申请任务</summary>
    /// <remarks>
    /// 业务应用根据使用场景，可重载Acquire并返回空来阻止创建新任务
    /// </remarks>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public override ITask[] Acquire(Int32 count)
    {
        // 消费模式，设置Topic值
        var prv = Provider;
        var job = Job;

        return prv.Acquire(job, Topic, count);
    }

    /// <summary>解码一批消息，处理任务</summary>
    /// <param name="ctx"></param>
    protected override void OnProcess(JobContext ctx)
    {
        if (ctx.Task.Data.IsNullOrEmpty()) return;

        var ss = ctx.Task.Data.ToJsonEntity<String[]>();
        if (ss == null || ss.Length == 0) return;

        ctx.Total = ss.Length;
        ctx.Data = ss;

        var span = DefaultSpan.Current;
        if (span != null) span.Value = ctx.Total;

        Execute(ctx);
    }

    /// <summary>根据解码后的消息执行任务</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    public override Int32 Execute(JobContext ctx)
    {
        var count = 0;
        foreach (String item in ctx.Data as IEnumerable)
        {
            if (ProcessItem(ctx, item)) count++;
        }

        return count;
    }

    /// <summary>处理一个数据对象</summary>
    /// <param name="ctx">上下文</param>
    /// <param name="message">消息</param>
    /// <returns></returns>
    public virtual Boolean ProcessItem(JobContext ctx, String message) => true;
    #endregion
}