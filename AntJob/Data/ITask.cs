using System;

namespace AntJob.Data
{
    /// <summary>任务参数</summary>
    public interface ITask
    {
        /// <summary>任务项编号</summary>
        Int64 ID { get; set; }

        /// <summary>开始。大于等于</summary>
        DateTime Start { get; set; }

        /// <summary>结束。小于</summary>
        DateTime End { get; set; }

        ///// <summary>时间偏移。距离实时时间的秒数，部分业务不能跑到实时</summary>
        //Int32 Offset { get; set; }

        ///// <summary>步进。最大区间大小，秒</summary>
        //Int32 Step { get; set; }

        /// <summary>批大小</summary>
        Int32 BatchSize { get; set; }

        /// <summary>状态</summary>
        JobStatus Status { get; set; }

        /// <summary>数据</summary>
        String Data { get; set; }

        /// <summary>内容</summary>
        String Message { get; set; }
    }

    public partial class TaskModel : ITask { }
}