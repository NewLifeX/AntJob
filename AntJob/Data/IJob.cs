namespace AntJob.Data;

/// <summary>作业参数</summary>
public interface IJob
{
    /// <summary>名称</summary>
    String Name { get; set; }

    /// <summary>类名。支持该作业的处理器实现</summary>
    String ClassName { get; set; }

    /// <summary>是否启用</summary>
    Boolean Enable { get; set; }

    /// <summary>数据时间。定时调度的执行时间点，或者数据调度的开始时间</summary>
    DateTime DataTime { get; set; }

    /// <summary>结束。小于该时间，数据作业使用</summary>
    DateTime End { get; set; }

    /// <summary>时间偏移。距离实时时间的秒数，考虑到服务器之间的时间差，部分业务不能跑到实时</summary>
    Int32 Offset { get; set; }

    /// <summary>步进。最大区间大小，秒</summary>
    Int32 Step { get; set; }

    /// <summary>批大小</summary>
    Int32 BatchSize { get; set; }

    /// <summary>并行度。最大同时执行任务数</summary>
    Int32 MaxTask { get; set; }

    /// <summary>调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</summary>
    JobModes Mode { get; set; }

    /// <summary>Cron定时表达式</summary>
    String Cron { get; set; }

    /// <summary>消息主题</summary>
    String Topic { get; set; }

    /// <summary>数据</summary>
    String Data { get; set; }
}

public partial class JobModel : IJob { }