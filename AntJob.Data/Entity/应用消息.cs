using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity
{
    /// <summary>应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理</summary>
    [Serializable]
    [DataObject]
    [Description("应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理")]
    [BindIndex("IX_AppMessage_AppID_Topic_UpdateTime", false, "AppID,Topic,UpdateTime")]
    [BindIndex("IX_AppMessage_UpdateTime", false, "UpdateTime")]
    [BindTable("AppMessage", Description = "应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理", ConnName = "Ant", DbType = DatabaseType.None)]
    public partial class AppMessage : IAppMessage
    {
        #region 属性
        private Int64 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int64 ID { get { return _ID; } set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private Int32 _AppID;
        /// <summary>应用</summary>
        [DisplayName("应用")]
        [Description("应用")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("AppID", "应用", "")]
        public Int32 AppID { get { return _AppID; } set { if (OnPropertyChanging(__.AppID, value)) { _AppID = value; OnPropertyChanged(__.AppID); } } }

        private Int32 _JobID;
        /// <summary>作业。生产消息的作业</summary>
        [DisplayName("作业")]
        [Description("作业。生产消息的作业")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("JobID", "作业。生产消息的作业", "")]
        public Int32 JobID { get { return _JobID; } set { if (OnPropertyChanging(__.JobID, value)) { _JobID = value; OnPropertyChanged(__.JobID); } } }

        private String _Topic;
        /// <summary>主题。区分作业下多种消息</summary>
        [DisplayName("主题")]
        [Description("主题。区分作业下多种消息")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Topic", "主题。区分作业下多种消息", "")]
        public String Topic { get { return _Topic; } set { if (OnPropertyChanging(__.Topic, value)) { _Topic = value; OnPropertyChanged(__.Topic); } } }

        private String _Data;
        /// <summary>数据。可以是Json数据，比如StatID</summary>
        [DisplayName("数据")]
        [Description("数据。可以是Json数据，比如StatID")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Data", "数据。可以是Json数据，比如StatID", "")]
        public String Data { get { return _Data; } set { if (OnPropertyChanging(__.Data, value)) { _Data = value; OnPropertyChanged(__.Data); } } }

        private DateTime _CreateTime;
        /// <summary>创建时间</summary>
        [DisplayName("创建时间")]
        [Description("创建时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("CreateTime", "创建时间", "")]
        public DateTime CreateTime { get { return _CreateTime; } set { if (OnPropertyChanging(__.CreateTime, value)) { _CreateTime = value; OnPropertyChanged(__.CreateTime); } } }

        private DateTime _UpdateTime;
        /// <summary>更新时间</summary>
        [DisplayName("更新时间")]
        [Description("更新时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("UpdateTime", "更新时间", "")]
        public DateTime UpdateTime { get { return _UpdateTime; } set { if (OnPropertyChanging(__.UpdateTime, value)) { _UpdateTime = value; OnPropertyChanged(__.UpdateTime); } } }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        public override Object this[String name]
        {
            get
            {
                switch (name)
                {
                    case __.ID : return _ID;
                    case __.AppID : return _AppID;
                    case __.JobID : return _JobID;
                    case __.Topic : return _Topic;
                    case __.Data : return _Data;
                    case __.CreateTime : return _CreateTime;
                    case __.UpdateTime : return _UpdateTime;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID : _ID = value.ToLong(); break;
                    case __.AppID : _AppID = value.ToInt(); break;
                    case __.JobID : _JobID = value.ToInt(); break;
                    case __.Topic : _Topic = Convert.ToString(value); break;
                    case __.Data : _Data = Convert.ToString(value); break;
                    case __.CreateTime : _CreateTime = value.ToDateTime(); break;
                    case __.UpdateTime : _UpdateTime = value.ToDateTime(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得应用消息字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>应用</summary>
            public static readonly Field AppID = FindByName(__.AppID);

            /// <summary>作业。生产消息的作业</summary>
            public static readonly Field JobID = FindByName(__.JobID);

            /// <summary>主题。区分作业下多种消息</summary>
            public static readonly Field Topic = FindByName(__.Topic);

            /// <summary>数据。可以是Json数据，比如StatID</summary>
            public static readonly Field Data = FindByName(__.Data);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            static Field FindByName(String name) { return Meta.Table.FindByName(name); }
        }

        /// <summary>取得应用消息字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

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

    /// <summary>应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理接口</summary>
    public partial interface IAppMessage
    {
        #region 属性
        /// <summary>编号</summary>
        Int64 ID { get; set; }

        /// <summary>应用</summary>
        Int32 AppID { get; set; }

        /// <summary>作业。生产消息的作业</summary>
        Int32 JobID { get; set; }

        /// <summary>主题。区分作业下多种消息</summary>
        String Topic { get; set; }

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        String Data { get; set; }

        /// <summary>创建时间</summary>
        DateTime CreateTime { get; set; }

        /// <summary>更新时间</summary>
        DateTime UpdateTime { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}