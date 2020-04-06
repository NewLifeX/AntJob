using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity
{
    /// <summary>作业任务</summary>
    [Serializable]
    [DataObject]
    [Description("作业任务")]
    [BindIndex("IX_JobTask_JobID_Status_Start", false, "JobID,Status,Start")]
    [BindIndex("IX_JobTask_AppID_Client_Status", false, "AppID,Client,Status")]
    [BindIndex("IX_JobTask_JobID_CreateTime", false, "JobID,CreateTime")]
    [BindTable("JobTask", Description = "作业任务", ConnName = "Ant", DbType = DatabaseType.None)]
    public partial class JobTask : IJobTask
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
        /// <summary>作业</summary>
        [DisplayName("作业")]
        [Description("作业")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("JobID", "作业", "")]
        public Int32 JobID { get { return _JobID; } set { if (OnPropertyChanging(__.JobID, value)) { _JobID = value; OnPropertyChanged(__.JobID); } } }

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

        private Int32 _Total;
        /// <summary>总数</summary>
        [DisplayName("总数")]
        [Description("总数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Total", "总数", "")]
        public Int32 Total { get { return _Total; } set { if (OnPropertyChanging(__.Total, value)) { _Total = value; OnPropertyChanged(__.Total); } } }

        private Int32 _Success;
        /// <summary>成功</summary>
        [DisplayName("成功")]
        [Description("成功")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Success", "成功", "")]
        public Int32 Success { get { return _Success; } set { if (OnPropertyChanging(__.Success, value)) { _Success = value; OnPropertyChanged(__.Success); } } }

        private Int32 _Error;
        /// <summary>错误</summary>
        [DisplayName("错误")]
        [Description("错误")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Error", "错误", "")]
        public Int32 Error { get { return _Error; } set { if (OnPropertyChanging(__.Error, value)) { _Error = value; OnPropertyChanged(__.Error); } } }

        private Int32 _Times;
        /// <summary>次数</summary>
        [DisplayName("次数")]
        [Description("次数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Times", "次数", "")]
        public Int32 Times { get { return _Times; } set { if (OnPropertyChanging(__.Times, value)) { _Times = value; OnPropertyChanged(__.Times); } } }

        private Int32 _Speed;
        /// <summary>速度</summary>
        [DisplayName("速度")]
        [Description("速度")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Speed", "速度", "")]
        public Int32 Speed { get { return _Speed; } set { if (OnPropertyChanging(__.Speed, value)) { _Speed = value; OnPropertyChanged(__.Speed); } } }

        private Int32 _Cost;
        /// <summary>耗时。秒</summary>
        [DisplayName("耗时")]
        [Description("耗时。秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Cost", "耗时。秒", "")]
        public Int32 Cost { get { return _Cost; } set { if (OnPropertyChanging(__.Cost, value)) { _Cost = value; OnPropertyChanged(__.Cost); } } }

        private Int32 _FullCost;
        /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
        [DisplayName("全部耗时")]
        [Description("全部耗时。秒，从任务发放到执行完成的时间")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("FullCost", "全部耗时。秒，从任务发放到执行完成的时间", "")]
        public Int32 FullCost { get { return _FullCost; } set { if (OnPropertyChanging(__.FullCost, value)) { _FullCost = value; OnPropertyChanged(__.FullCost); } } }

        private JobStatus _Status;
        /// <summary>状态</summary>
        [DisplayName("状态")]
        [Description("状态")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Status", "状态", "")]
        public JobStatus Status { get { return _Status; } set { if (OnPropertyChanging(__.Status, value)) { _Status = value; OnPropertyChanged(__.Status); } } }

        private Int32 _MsgCount;
        /// <summary>消费消息数</summary>
        [DisplayName("消费消息数")]
        [Description("消费消息数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MsgCount", "消费消息数", "")]
        public Int32 MsgCount { get { return _MsgCount; } set { if (OnPropertyChanging(__.MsgCount, value)) { _MsgCount = value; OnPropertyChanged(__.MsgCount); } } }

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

        private String _Key;
        /// <summary>最后键</summary>
        [DisplayName("最后键")]
        [Description("最后键")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Key", "最后键", "")]
        public String Key { get { return _Key; } set { if (OnPropertyChanging(__.Key, value)) { _Key = value; OnPropertyChanged(__.Key); } } }

        private String _Data;
        /// <summary>数据。可以是Json数据，比如StatID</summary>
        [DisplayName("数据")]
        [Description("数据。可以是Json数据，比如StatID")]
        [DataObjectField(false, false, true, 8000)]
        [BindColumn("Data", "数据。可以是Json数据，比如StatID", "")]
        public String Data { get { return _Data; } set { if (OnPropertyChanging(__.Data, value)) { _Data = value; OnPropertyChanged(__.Data); } } }

        private String _Message;
        /// <summary>备注</summary>
        [DisplayName("备注")]
        [Description("备注")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Message", "备注", "")]
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
                    case __.Client : return _Client;
                    case __.Start : return _Start;
                    case __.End : return _End;
                    case __.BatchSize : return _BatchSize;
                    case __.Total : return _Total;
                    case __.Success : return _Success;
                    case __.Error : return _Error;
                    case __.Times : return _Times;
                    case __.Speed : return _Speed;
                    case __.Cost : return _Cost;
                    case __.FullCost : return _FullCost;
                    case __.Status : return _Status;
                    case __.MsgCount : return _MsgCount;
                    case __.Server : return _Server;
                    case __.ProcessID : return _ProcessID;
                    case __.Key : return _Key;
                    case __.Data : return _Data;
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
                    case __.ID : _ID = value.ToLong(); break;
                    case __.AppID : _AppID = value.ToInt(); break;
                    case __.JobID : _JobID = value.ToInt(); break;
                    case __.Client : _Client = Convert.ToString(value); break;
                    case __.Start : _Start = value.ToDateTime(); break;
                    case __.End : _End = value.ToDateTime(); break;
                    case __.BatchSize : _BatchSize = value.ToInt(); break;
                    case __.Total : _Total = value.ToInt(); break;
                    case __.Success : _Success = value.ToInt(); break;
                    case __.Error : _Error = value.ToInt(); break;
                    case __.Times : _Times = value.ToInt(); break;
                    case __.Speed : _Speed = value.ToInt(); break;
                    case __.Cost : _Cost = value.ToInt(); break;
                    case __.FullCost : _FullCost = value.ToInt(); break;
                    case __.Status : _Status = (JobStatus)value.ToInt(); break;
                    case __.MsgCount : _MsgCount = value.ToInt(); break;
                    case __.Server : _Server = Convert.ToString(value); break;
                    case __.ProcessID : _ProcessID = value.ToInt(); break;
                    case __.Key : _Key = Convert.ToString(value); break;
                    case __.Data : _Data = Convert.ToString(value); break;
                    case __.Message : _Message = Convert.ToString(value); break;
                    case __.CreateTime : _CreateTime = value.ToDateTime(); break;
                    case __.UpdateTime : _UpdateTime = value.ToDateTime(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得作业任务字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>应用</summary>
            public static readonly Field AppID = FindByName(__.AppID);

            /// <summary>作业</summary>
            public static readonly Field JobID = FindByName(__.JobID);

            /// <summary>客户端。IP加进程</summary>
            public static readonly Field Client = FindByName(__.Client);

            /// <summary>开始。大于等于</summary>
            public static readonly Field Start = FindByName(__.Start);

            /// <summary>结束。小于，不等于</summary>
            public static readonly Field End = FindByName(__.End);

            /// <summary>批大小</summary>
            public static readonly Field BatchSize = FindByName(__.BatchSize);

            /// <summary>总数</summary>
            public static readonly Field Total = FindByName(__.Total);

            /// <summary>成功</summary>
            public static readonly Field Success = FindByName(__.Success);

            /// <summary>错误</summary>
            public static readonly Field Error = FindByName(__.Error);

            /// <summary>次数</summary>
            public static readonly Field Times = FindByName(__.Times);

            /// <summary>速度</summary>
            public static readonly Field Speed = FindByName(__.Speed);

            /// <summary>耗时。秒</summary>
            public static readonly Field Cost = FindByName(__.Cost);

            /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
            public static readonly Field FullCost = FindByName(__.FullCost);

            /// <summary>状态</summary>
            public static readonly Field Status = FindByName(__.Status);

            /// <summary>消费消息数</summary>
            public static readonly Field MsgCount = FindByName(__.MsgCount);

            /// <summary>服务器</summary>
            public static readonly Field Server = FindByName(__.Server);

            /// <summary>进程</summary>
            public static readonly Field ProcessID = FindByName(__.ProcessID);

            /// <summary>最后键</summary>
            public static readonly Field Key = FindByName(__.Key);

            /// <summary>数据。可以是Json数据，比如StatID</summary>
            public static readonly Field Data = FindByName(__.Data);

            /// <summary>备注</summary>
            public static readonly Field Message = FindByName(__.Message);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            static Field FindByName(String name) { return Meta.Table.FindByName(name); }
        }

        /// <summary>取得作业任务字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>应用</summary>
            public const String AppID = "AppID";

            /// <summary>作业</summary>
            public const String JobID = "JobID";

            /// <summary>客户端。IP加进程</summary>
            public const String Client = "Client";

            /// <summary>开始。大于等于</summary>
            public const String Start = "Start";

            /// <summary>结束。小于，不等于</summary>
            public const String End = "End";

            /// <summary>批大小</summary>
            public const String BatchSize = "BatchSize";

            /// <summary>总数</summary>
            public const String Total = "Total";

            /// <summary>成功</summary>
            public const String Success = "Success";

            /// <summary>错误</summary>
            public const String Error = "Error";

            /// <summary>次数</summary>
            public const String Times = "Times";

            /// <summary>速度</summary>
            public const String Speed = "Speed";

            /// <summary>耗时。秒</summary>
            public const String Cost = "Cost";

            /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
            public const String FullCost = "FullCost";

            /// <summary>状态</summary>
            public const String Status = "Status";

            /// <summary>消费消息数</summary>
            public const String MsgCount = "MsgCount";

            /// <summary>服务器</summary>
            public const String Server = "Server";

            /// <summary>进程</summary>
            public const String ProcessID = "ProcessID";

            /// <summary>最后键</summary>
            public const String Key = "Key";

            /// <summary>数据。可以是Json数据，比如StatID</summary>
            public const String Data = "Data";

            /// <summary>备注</summary>
            public const String Message = "Message";

            /// <summary>创建时间</summary>
            public const String CreateTime = "CreateTime";

            /// <summary>更新时间</summary>
            public const String UpdateTime = "UpdateTime";
        }
        #endregion
    }

    /// <summary>作业任务接口</summary>
    public partial interface IJobTask
    {
        #region 属性
        /// <summary>编号</summary>
        Int64 ID { get; set; }

        /// <summary>应用</summary>
        Int32 AppID { get; set; }

        /// <summary>作业</summary>
        Int32 JobID { get; set; }

        /// <summary>客户端。IP加进程</summary>
        String Client { get; set; }

        /// <summary>开始。大于等于</summary>
        DateTime Start { get; set; }

        /// <summary>结束。小于，不等于</summary>
        DateTime End { get; set; }

        /// <summary>批大小</summary>
        Int32 BatchSize { get; set; }

        /// <summary>总数</summary>
        Int32 Total { get; set; }

        /// <summary>成功</summary>
        Int32 Success { get; set; }

        /// <summary>错误</summary>
        Int32 Error { get; set; }

        /// <summary>次数</summary>
        Int32 Times { get; set; }

        /// <summary>速度</summary>
        Int32 Speed { get; set; }

        /// <summary>耗时。秒</summary>
        Int32 Cost { get; set; }

        /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
        Int32 FullCost { get; set; }

        /// <summary>状态</summary>
        JobStatus Status { get; set; }

        /// <summary>消费消息数</summary>
        Int32 MsgCount { get; set; }

        /// <summary>服务器</summary>
        String Server { get; set; }

        /// <summary>进程</summary>
        Int32 ProcessID { get; set; }

        /// <summary>最后键</summary>
        String Key { get; set; }

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        String Data { get; set; }

        /// <summary>备注</summary>
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