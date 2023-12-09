using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity;

/// <summary>应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理</summary>
[Serializable]
[DataObject]
[Description("应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理")]
[BindIndex("IX_AppMessage_AppID_Topic_UpdateTime", false, "AppID,Topic,UpdateTime")]
[BindTable("AppMessage", Description = "应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class AppMessage
{
    #region 属性
    private Int64 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, false, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int64 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private Int32 _AppID;
    /// <summary>应用</summary>
    [DisplayName("应用")]
    [Description("应用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AppID", "应用", "")]
    public Int32 AppID { get => _AppID; set { if (OnPropertyChanging("AppID", value)) { _AppID = value; OnPropertyChanged("AppID"); } } }

    private Int32 _JobID;
    /// <summary>作业。生产消息的作业</summary>
    [DisplayName("作业")]
    [Description("作业。生产消息的作业")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("JobID", "作业。生产消息的作业", "")]
    public Int32 JobID { get => _JobID; set { if (OnPropertyChanging("JobID", value)) { _JobID = value; OnPropertyChanged("JobID"); } } }

    private String _Topic;
    /// <summary>主题。区分作业下多种消息</summary>
    [DisplayName("主题")]
    [Description("主题。区分作业下多种消息")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Topic", "主题。区分作业下多种消息", "")]
    public String Topic { get => _Topic; set { if (OnPropertyChanging("Topic", value)) { _Topic = value; OnPropertyChanged("Topic"); } } }

    private String _Data;
    /// <summary>数据。可以是Json数据，比如StatID</summary>
    [DisplayName("数据")]
    [Description("数据。可以是Json数据，比如StatID")]
    [DataObjectField(false, false, true, 2000)]
    [BindColumn("Data", "数据。可以是Json数据，比如StatID", "")]
    public String Data { get => _Data; set { if (OnPropertyChanging("Data", value)) { _Data = value; OnPropertyChanged("Data"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
        get => name switch
        {
            "Id" => _Id,
            "AppID" => _AppID,
            "JobID" => _JobID,
            "Topic" => _Topic,
            "Data" => _Data,
            "CreateTime" => _CreateTime,
            "UpdateTime" => _UpdateTime,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToLong(); break;
                case "AppID": _AppID = value.ToInt(); break;
                case "JobID": _JobID = value.ToInt(); break;
                case "Topic": _Topic = Convert.ToString(value); break;
                case "Data": _Data = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    #endregion

    #region 字段名
    /// <summary>取得应用消息字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>应用</summary>
        public static readonly Field AppID = FindByName("AppID");

        /// <summary>作业。生产消息的作业</summary>
        public static readonly Field JobID = FindByName("JobID");

        /// <summary>主题。区分作业下多种消息</summary>
        public static readonly Field Topic = FindByName("Topic");

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        public static readonly Field Data = FindByName("Data");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得应用消息字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>应用</summary>
        public const String AppID = "AppID";

        /// <summary>作业。生产消息的作业</summary>
        public const String JobID = "JobID";

        /// <summary>主题。区分作业下多种消息</summary>
        public const String Topic = "Topic";

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        public const String Data = "Data";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";
    }
    #endregion
}
