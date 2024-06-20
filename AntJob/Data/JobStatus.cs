namespace AntJob.Data;

/// <summary>作业状态</summary>
public enum JobStatus
{
    /// <summary>就绪</summary>
    就绪 = 0,

    /// <summary>抽取中</summary>
    抽取中 = 1,

    /// <summary>处理中</summary>
    处理中 = 2,

    /// <summary>错误</summary>
    错误 = 3,

    /// <summary>已完成</summary>
    完成 = 4,

    /// <summary>已取消</summary>
    取消 = 5,

    /// <summary>延迟重试</summary>
    延迟 = 6,
}