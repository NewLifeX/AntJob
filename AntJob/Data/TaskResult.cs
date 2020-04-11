using System;

namespace AntJob.Data
{
    /// <summary>任务结果</summary>
    public partial class TaskResult : ITaskResult
    {
        #region 属性
        /// <summary>编号</summary>
        public Int32 ID { get; set; }

        /// <summary>总数</summary>
        public Int32 Total { get; set; }

        /// <summary>成功</summary>
        public Int32 Success { get; set; }

        /// <summary>耗时，秒</summary>
        public Int32 Cost { get; set; }

        /// <summary>错误</summary>
        public Int32 Error { get; set; }

        /// <summary>次数</summary>
        public Int32 Times { get; set; }

        /// <summary>速度</summary>
        public Int32 Speed { get; set; }

        /// <summary>状态</summary>
        public JobStatus Status { get; set; }

        /// <summary>最后键值</summary>
        public String Key { get; set; }

        /// <summary>消息内容。异常信息或其它任务消息</summary>
        public String Message { get; set; }
        #endregion
    }
}