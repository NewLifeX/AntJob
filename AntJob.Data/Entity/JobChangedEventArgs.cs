using System;

namespace AntJob.Data.Entity;

/// <summary>作业调度参数变更事件参数</summary>
/// <remarks>仅当影响调度的字段发生变化时触发，统计字段更新不会触发</remarks>
public class JobChangedEventArgs(Job job, String action) : EventArgs
{
    /// <summary>发生变更的作业</summary>
    public Job Job { get; } = job;

    /// <summary>变更动作。Insert/Update/Delete</summary>
    public String Action { get; } = action;
}
