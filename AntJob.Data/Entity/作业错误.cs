using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity
{
    /// <summary>作业错误</summary>
    [Serializable]
    [DataObject]
    [Description("作业错误")]
    [BindIndex("IX_JobError_AppID_ID", false, "AppID,ID")]
    [BindIndex("IX_JobError_JobID_ID", false, "JobID,ID")]
    [BindIndex("IX_JobError_Key", false, "Key")]
    [BindIndex("IX_JobError_ErrorCode", false, "ErrorCode")]
    [BindTable("JobError", Description = "作业错误", ConnName = "Ant", DbType = DatabaseType.None)]
    public partial class JobError : IJobError
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get { return _ID; } set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private Int32 _AppID;
        /// <summary>应用</summary>
        [DisplayName("应用")]
        [Description("应用")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("AppID", "应用", "")]
        public Int32 AppID { get { return _AppID; } set { if (OnPropertyChanging(__.AppID, value)) { _AppID = value; OnPropertyChanged(__.AppID); } } }

        private Int32 _JobID;
        /// <summary>作业</summary>
        [DisplayName("作业")]
        [Description("作业")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("JobID", "作业", "")]
        public Int32 JobID { get { return _JobID; } set { if (OnPropertyChanging(__.JobID, value)) { _JobID = value; OnPropertyChanged(__.JobID); } } }

        private Int32 _TaskID;
        /// <summary>作业项</summary>
        [DisplayName("作业项")]
        [Description("作业项")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("TaskID", "作业项", "")]
        public Int32 TaskID { get { return _TaskID; } set { if (OnPropertyChanging(__.TaskID, value)) { _TaskID = value; OnPropertyChanged(__.TaskID); } } }

        private String _Client;
        /// <summary>客户端。IP加进程</summary>
        [DisplayName("客户端")]
        [Description("客户端。IP加进程")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Client", "客户端。IP加进程", "")]
        public String Client { get { return _Client; } set { if (OnPropertyChanging(__.Client, value)) { _Client = value; OnPropertyChanged(__.Client); } } }

        private DateTime _Start;
        /// <summary>开始。大于等于</summary>
        [DisplayName("开始")]
        [Description("开始。大于等于")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("Start", "开始。大于等于", "")]
        public DateTime Start { get { return _Start; } set { if (OnPropertyChanging(__.Start, value)) { _Start = value; OnPropertyChanged(__.Start); } } }

        private DateTime _End;
        /// <summary>结束。小于，不等于</summary>
        [DisplayName("结束")]
        [Description("结束。小于，不等于")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("End", "结束。小于，不等于", "")]
        public DateTime End { get { return _End; } set { if (OnPropertyChanging(__.End, value)) { _End = value; OnPropertyChanged(__.End); } } }

        private Int32 _BatchSize;
        /// <summary>批大小</summary>
        [DisplayName("批大小")]
        [Description("批大小")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("BatchSize", "批大小", "")]
        public Int32 BatchSize { get { return _BatchSize; } set { if (OnPropertyChanging(__.BatchSize, value)) { _BatchSize = value; OnPropertyChanged(__.BatchSize); } } }

        private String _Key;
        /// <summary>数据键</summary>
        [DisplayName("数据键")]
        [Description("数据键")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Key", "数据键", "")]
        public String Key { get { return _Key; } set { if (OnPropertyChanging(__.Key, value)) { _Key = value; OnPropertyChanged(__.Key); } } }

        private String _Data;
        /// <summary>数据</summary>
        [DisplayName("数据")]
        [Description("数据")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Data", "数据", "")]
        public String Data { get { return _Data; } set { if (OnPropertyChanging(__.Data, value)) { _Data = value; OnPropertyChanged(__.Data); } } }

        private String _Server;
        /// <summary>服务器</summary>
        [DisplayName("服务器")]
        [Description("服务器")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Server", "服务器", "")]
        public String Server { get { return _Server; } set { if (OnPropertyChanging(__.Server, value)) { _Server = value; OnPropertyChanged(__.Server); } } }

        private Int32 _ProcessID;
        /// <summary>进程</summary>
        [DisplayName("进程")]
        [Description("进程")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ProcessID", "进程", "")]
        public Int32 ProcessID { get { return _ProcessID; } set { if (OnPropertyChanging(__.ProcessID, value)) { _ProcessID = value; OnPropertyChanged(__.ProcessID); } } }

        private String _ErrorCode;
        /// <summary>错误码</summary>
        [DisplayName("错误码")]
        [Description("错误码")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("ErrorCode", "错误码", "")]
        public String ErrorCode { get { return _ErrorCode; } set { if (OnPropertyChanging(__.ErrorCode, value)) { _ErrorCode = value; OnPropertyChanged(__.ErrorCode); } } }

        private String _Message;
        /// <summary>内容</summary>
        [DisplayName("内容")]
        [Description("内容")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Message", "内容", "")]
        public String Message { get { return _Message; } set { if (OnPropertyChanging(__.Message, value)) { _Message = value; OnPropertyChanged(__.Message); } } }

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
                    case __.TaskID : return _TaskID;
                    case __.Client : return _Client;
                    case __.Start : return _Start;
                    case __.End : return _End;
                    case __.BatchSize : return _BatchSize;
                    case __.Key : return _Key;
                    case __.Data : return _Data;
                    case __.Server : return _Server;
                    case __.ProcessID : return _ProcessID;
                    case __.ErrorCode : return _ErrorCode;
                    case __.Message : return _Message;
                    case __.CreateTime : return _CreateTime;
                    case __.UpdateTime : return _UpdateTime;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID : _ID = value.ToInt(); break;
                    case __.AppID : _AppID = value.ToInt(); break;
                    case __.JobID : _JobID = value.ToInt(); break;
                    case __.TaskID : _TaskID = value.ToInt(); break;
                    case __.Client : _Client = Convert.ToString(value); break;
                    case __.Start : _Start = value.ToDateTime(); break;
                    case __.End : _End = value.ToDateTime(); break;
                    case __.BatchSize : _BatchSize = value.ToInt(); break;
                    case __.Key : _Key = Convert.ToString(value); break;
                    case __.Data : _Data = Convert.ToString(value); break;
                    case __.Server : _Server = Convert.ToString(value); break;
                    case __.ProcessID : _ProcessID = value.ToInt(); break;
                    case __.ErrorCode : _ErrorCode = Convert.ToString(value); break;
                    case __.Message : _Message = Convert.ToString(value); break;
                    case __.CreateTime : _CreateTime = value.ToDateTime(); break;
                    case __.UpdateTime : _UpdateTime = value.ToDateTime(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得作业错误字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>应用</summary>
            public static readonly Field AppID = FindByName(__.AppID);

            /// <summary>作业</summary>
            public static readonly Field JobID = FindByName(__.JobID);

            /// <summary>作业项</summary>
            public static readonly Field TaskID = FindByName(__.TaskID);

            /// <summary>客户端。IP加进程</summary>
            public static readonly Field Client = FindByName(__.Client);

            /// <summary>开始。大于等于</summary>
            public static readonly Field Start = FindByName(__.Start);

            /// <summary>结束。小于，不等于</summary>
            public static readonly Field End = FindByName(__.End);

            /// <summary>批大小</summary>
            public static readonly Field BatchSize = FindByName(__.BatchSize);

            /// <summary>数据键</summary>
            public static readonly Field Key = FindByName(__.Key);

            /// <summary>数据</summary>
            public static readonly Field Data = FindByName(__.Data);

            /// <summary>服务器</summary>
            public static readonly Field Server = FindByName(__.Server);

            /// <summary>进程</summary>
            public static readonly Field ProcessID = FindByName(__.ProcessID);

            /// <summary>错误码</summary>
            public static readonly Field ErrorCode = FindByName(__.ErrorCode);

            /// <summary>内容</summary>
            public static readonly Field Message = FindByName(__.Message);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            static Field FindByName(String name) { return Meta.Table.FindByName(name); }
        }

        /// <summary>取得作业错误字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>应用</summary>
            public const String AppID = "AppID";

            /// <summary>作业</summary>
            public const String JobID = "JobID";

            /// <summary>作业项</summary>
            public const String TaskID = "TaskID";

            /// <summary>客户端。IP加进程</summary>
            public const String Client = "Client";

            /// <summary>开始。大于等于</summary>
            public const String Start = "Start";

            /// <summary>结束。小于，不等于</summary>
            public const String End = "End";

            /// <summary>批大小</summary>
            public const String BatchSize = "BatchSize";

            /// <summary>数据键</summary>
            public const String Key = "Key";

            /// <summary>数据</summary>
            public const String Data = "Data";

            /// <summary>服务器</summary>
            public const String Server = "Server";

            /// <summary>进程</summary>
            public const String ProcessID = "ProcessID";

            /// <summary>错误码</summary>
            public const String ErrorCode = "ErrorCode";

            /// <summary>内容</summary>
            public const String Message = "Message";

            /// <summary>创建时间</summary>
            public const String CreateTime = "CreateTime";

            /// <summary>更新时间</summary>
            public const String UpdateTime = "UpdateTime";
        }
        #endregion
    }

    /// <summary>作业错误接口</summary>
    public partial interface IJobError
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>应用</summary>
        Int32 AppID { get; set; }

        /// <summary>作业</summary>
        Int32 JobID { get; set; }

        /// <summary>作业项</summary>
        Int32 TaskID { get; set; }

        /// <summary>客户端。IP加进程</summary>
        String Client { get; set; }

        /// <summary>开始。大于等于</summary>
        DateTime Start { get; set; }

        /// <summary>结束。小于，不等于</summary>
        DateTime End { get; set; }

        /// <summary>批大小</summary>
        Int32 BatchSize { get; set; }

        /// <summary>数据键</summary>
        String Key { get; set; }

        /// <summary>数据</summary>
        String Data { get; set; }

        /// <summary>服务器</summary>
        String Server { get; set; }

        /// <summary>进程</summary>
        Int32 ProcessID { get; set; }

        /// <summary>错误码</summary>
        String ErrorCode { get; set; }

        /// <summary>内容</summary>
        String Message { get; set; }

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