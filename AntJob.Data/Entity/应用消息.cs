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
[BindIndex("IX_AppMessage_AppID_Topic_DelayTime", false, "AppID,Topic,DelayTime")]
[BindTable("AppMessage", Description = "应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class AppMessage
{
    #region 属性
    private Int64 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, false, false, 0)]
    [BindColumn("Id", "编号", "", DataScale = "time")]
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

    private DateTime _DelayTime;
    /// <summary>延迟时间。延迟到该时间执行</summary>
    [DisplayName("延迟时间")]
    [Description("延迟时间。延迟到该时间执行")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("DelayTime", "延迟时间。延迟到该时间执行", "")]
    public DateTime DelayTime { get => _DelayTime; set { if (OnPropertyChanging("DelayTime", value)) { _DelayTime = value; OnPropertyChanged("DelayTime"); } } }

    private String _TraceId;
    /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
    [Category("扩展")]
    [DisplayName("追踪")]
    [Description("追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("TraceId", "追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链", "")]
    public String TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [Category("扩展")]
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [Category("扩展")]
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
    [Category("扩展")]
    [DisplayName("更新地址")]
    [Description("更新地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新地址", "")]
    public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [Category("扩展")]
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
            "DelayTime" => _DelayTime,
            "TraceId" => _TraceId,
            "CreateIP" => _CreateIP,
            "CreateTime" => _CreateTime,
            "UpdateIP" => _UpdateIP,
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
                case "DelayTime": _DelayTime = value.ToDateTime(); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    /// <summary>应用</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public App App => Extends.Get(nameof(App), k => App.FindByID(AppID));

    /// <summary>应用</summary>
    [Map(nameof(AppID), typeof(App), "ID")]
    public String AppName => App?.ToString();

    /// <summary>作业</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public Job Job => Extends.Get(nameof(Job), k => Job.FindByID(JobID));

    /// <summary>作业</summary>
    [Map(nameof(JobID), typeof(Job), "ID")]
    public String JobName => Job?.ToString();

    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static AppMessage FindById(Int64 id)
    {
        if (id < 0) return null;

        return Find(_.Id == id);
    }

    /// <summary>根据应用查找</summary>
    /// <param name="appId">应用</param>
    /// <returns>实体列表</returns>
    public static IList<AppMessage> FindAllByAppID(Int32 appId)
    {
        if (appId < 0) return [];

        return FindAll(_.AppID == appId);
    }
    #endregion

    #region 数据清理
    /// <summary>清理指定时间段内的数据</summary>
    /// <param name="start">开始时间。未指定时清理小于指定时间的所有数据</param>
    /// <param name="end">结束时间</param>
    /// <returns>清理行数</returns>
    public static Int32 DeleteWith(DateTime start, DateTime end)
    {
        return Delete(_.Id.Between(start, end, Meta.Factory.Snow));
    }
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

        /// <summary>延迟时间。延迟到该时间执行</summary>
        public static readonly Field DelayTime = FindByName("DelayTime");

        /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

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

        /// <summary>延迟时间。延迟到该时间执行</summary>
        public const String DelayTime = "DelayTime";

        /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
        public const String TraceId = "TraceId";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";
    }
    #endregion
}
