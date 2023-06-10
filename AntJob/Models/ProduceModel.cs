namespace AntJob.Models;

/// <summary>生成消息的模型</summary>
public class ProduceModel
{
    /// <summary>作业名</summary>
    public String Job { get; set; }

    /// <summary>主题</summary>
    public String Topic { get; set; }

    /// <summary>消息集合</summary>
    public String[] Messages { get; set; }

    /// <summary>延迟执行间隔。（实际执行时间=延迟+生产时间），单位秒</summary>
    public Int32 DelayTime { get; set; }

    /// <summary>消息去重。避免单个消息被重复生产</summary>
    public Boolean Unique { get; set; }
}