using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace AntJob
{
    /// <summary>作业参数</summary>
    public interface IJob
    {
        /// <summary>名称</summary>
        String Name { get; set; }

        /// <summary>是否启用</summary>
        Boolean Enable { get; set; }

        /// <summary>开始。大于等于该时间，定时作业和数据作业使用</summary>
        DateTime Start { get; set; }

        /// <summary>结束。小于该时间，数据作业使用</summary>
        DateTime End { get; set; }

        /// <summary>时间偏移。距离实时时间的秒数，部分业务不能跑到实时</summary>
        Int32 Offset { get; set; }

        /// <summary>步进。最大区间大小，秒</summary>
        Int32 Step { get; set; }

        /// <summary>批大小</summary>
        Int32 BatchSize { get; set; }

        /// <summary>并行度。最大同时执行任务数</summary>
        Int32 MaxTask { get; set; }

        /// <summary>调度模式</summary>
        JobModes Mode { get; set; }
    }

    /// <summary>作业参数</summary>
    [DebuggerDisplay("{Name} {Enable} {Start}")]
    public class MyJob : IJob
    {
        #region 属性
        /// <summary>名称</summary>
        [XmlAttribute]
        public String Name { get; set; }

        /// <summary>是否启用</summary>
        [XmlAttribute]
        public Boolean Enable { get; set; }

        /// <summary>开始。大于等于</summary>
        [XmlAttribute]
        public DateTime Start { get; set; }

        /// <summary>结束。小于</summary>
        [XmlAttribute]
        public DateTime End { get; set; }

        /// <summary>时间偏移。距离实时时间的秒数，部分业务不能跑到实时</summary>
        [XmlAttribute]
        public Int32 Offset { get; set; }

        /// <summary>步进。最大区间大小，秒</summary>
        [XmlAttribute]
        public Int32 Step { get; set; }

        /// <summary>批大小</summary>
        [XmlAttribute]
        public Int32 BatchSize { get; set; } = 5000;

        /// <summary>最大任务数</summary>
        [XmlAttribute]
        public Int32 MaxTask { get; set; }

        /// <summary>调度模式</summary>
        [XmlAttribute]
        public JobModes Mode { get; set; }

        /// <summary>显示名</summary>
        [XmlAttribute]
        public String DisplayName { get; set; }

        /// <summary>描述</summary>
        [XmlAttribute]
        public String Description { get; set; }
        #endregion

        #region 构造
        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override String ToString() => Name;
        #endregion
    }
}