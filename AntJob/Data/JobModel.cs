using System.Xml.Serialization;

namespace AntJob.Data;

/// <summary>作业模型</summary>
/// <remarks>定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</remarks>
public partial class JobModel : ICloneable
{
    #region 属性
    /// <summary>名称</summary>
    [XmlAttribute]
    public String Name { get; set; }

    /// <summary>类名。支持该作业的处理器实现</summary>
    [XmlAttribute]
    public String ClassName { get; set; }

    /// <summary>是否启用</summary>
    [XmlAttribute]
    public Boolean Enable { get; set; }

    /// <summary>数据时间。定时调度的执行时间点，或者数据调度的开始时间</summary>
    [XmlAttribute]
    public DateTime DataTime { get; set; }

    /// <summary>开始时间。兼容旧版</summary>
    [XmlAttribute]
    [Obsolete]
    public DateTime Start { get; set; }

    /// <summary>结束。小于</summary>
    [XmlAttribute]
    public DateTime End { get; set; }

    /// <summary>时间偏移。距离实时时间的秒数，考虑到服务器之间的时间差，部分业务不能跑到实时</summary>
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

    /// <summary>调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</summary>
    [XmlAttribute]
    public JobModes Mode { get; set; }

    /// <summary>显示名</summary>
    [XmlAttribute]
    public String DisplayName { get; set; }

    /// <summary>描述</summary>
    [XmlAttribute]
    public String Description { get; set; }

    /// <summary>Cron定时表达式</summary>
    [XmlAttribute]
    public String Cron { get; set; }

    /// <summary>消息主题</summary>
    [XmlAttribute]
    public String Topic { get; set; }

    /// <summary>数据</summary>
    [XmlAttribute]
    public String Data { get; set; }
    #endregion

    #region 构造
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Name;

    Object ICloneable.Clone() => MemberwiseClone();
    #endregion
}