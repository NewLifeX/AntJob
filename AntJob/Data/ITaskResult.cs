namespace AntJob.Data;

/// <summary>任务结果</summary>
public interface ITaskResult
{
    /// <summary>任务项编号</summary>
    Int32 ID { get; set; }

    /// <summary>状态</summary>
    JobStatus Status { get; set; }

    /// <summary>消息内容。异常信息或其它任务消息</summary>
    String Message { get; set; }
}