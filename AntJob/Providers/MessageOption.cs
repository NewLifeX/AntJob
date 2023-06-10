namespace AntJob.Providers;

/// <summary>消息选项</summary>
public class MessageOption
{
    /// <summary>延迟执行间隔（实际执行时间=延迟+生产时间），单位秒</summary>
    public Int32 DelayTime { get; set; }

    /// <summary>消息去重。避免单个消息被重复生产</summary>
    public Boolean Unique { get; set; }
}