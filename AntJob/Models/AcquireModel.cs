namespace AntJob.Models;

/// <summary>申请作业任务</summary>
public class AcquireModel
{
    /// <summary>作业名</summary>
    public String Job { get; set; }

    /// <summary>主题</summary>
    public String Topic { get; set; }

    /// <summary>任务数</summary>
    public Int32 Count { get; set; }
}
