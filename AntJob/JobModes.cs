using System.ComponentModel;

namespace AntJob
{
    /// <summary>作业模式</summary>
    //[Description("作业模式")]
    public enum JobModes
    {
        /// <summary>数据调度</summary>
        [Description("数据调度")]
        Data = 1,

        /// <summary>定时调度</summary>
        [Description("定时调度")]
        Alarm = 2,

        /// <summary>消息调度</summary>
        [Description("消息调度")]
        Message = 3,
    }
}