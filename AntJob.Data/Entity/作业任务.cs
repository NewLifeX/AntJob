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

/// <summary>作业任务。计算作业在执行过程中生成的任务实例，具有该次执行所需参数</summary>
[Serializable]
[DataObject]
[Description("作业任务。计算作业在执行过程中生成的任务实例，具有该次执行所需参数")]
[BindIndex("IX_JobTask_JobID_DataTime", false, "JobID,DataTime")]
[BindIndex("IX_JobTask_AppID_Client_Status", false, "AppID,Client,Status")]
[BindIndex("IX_JobTask_JobID_CreateTime", false, "JobID,CreateTime")]
[BindTable("JobTask", Description = "作业任务。计算作业在执行过程中生成的任务实例，具有该次执行所需参数", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class JobTask
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private Int32 _AppID;
    /// <summary>应用</summary>
    [DisplayName("应用")]
    [Description("应用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("AppID", "应用", "")]
    public Int32 AppID { get => _AppID; set { if (OnPropertyChanging("AppID", value)) { _AppID = value; OnPropertyChanged("AppID"); } } }

    private Int32 _JobID;
    /// <summary>作业</summary>
    [DisplayName("作业")]
    [Description("作业")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("JobID", "作业", "")]
    public Int32 JobID { get => _JobID; set { if (OnPropertyChanging("JobID", value)) { _JobID = value; OnPropertyChanged("JobID"); } } }

    private String _Client;
    /// <summary>客户端。IP加进程</summary>
    [DisplayName("客户端")]
    [Description("客户端。IP加进程")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Client", "客户端。IP加进程", "")]
    public String Client { get => _Client; set { if (OnPropertyChanging("Client", value)) { _Client = value; OnPropertyChanged("Client"); } } }

    private DateTime _DataTime;
    /// <summary>数据时间。大于等于，定时调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
    [DisplayName("数据时间")]
    [Description("数据时间。大于等于，定时调度到达该时间点后触发（可能有偏移量），消息调度不适用")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("DataTime", "数据时间。大于等于，定时调度到达该时间点后触发（可能有偏移量），消息调度不适用", "", DataScale = "time", Master = true)]
    public DateTime DataTime { get => _DataTime; set { if (OnPropertyChanging("DataTime", value)) { _DataTime = value; OnPropertyChanged("DataTime"); } } }

    private DateTime _End;
    /// <summary>结束。小于不等于，数据调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
    [DisplayName("结束")]
    [Description("结束。小于不等于，数据调度到达该时间点后触发（可能有偏移量），消息调度不适用")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("End", "结束。小于不等于，数据调度到达该时间点后触发（可能有偏移量），消息调度不适用", "")]
    public DateTime End { get => _End; set { if (OnPropertyChanging("End", value)) { _End = value; OnPropertyChanged("End"); } } }

    private Int32 _BatchSize;
    /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
    [DisplayName("批大小")]
    [Description("批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("BatchSize", "批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用", "")]
    public Int32 BatchSize { get => _BatchSize; set { if (OnPropertyChanging("BatchSize", value)) { _BatchSize = value; OnPropertyChanged("BatchSize"); } } }

    private Int32 _Total;
    /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
    [DisplayName("总数")]
    [Description("总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Total", "总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1", "")]
    public Int32 Total { get => _Total; set { if (OnPropertyChanging("Total", value)) { _Total = value; OnPropertyChanged("Total"); } } }

    private Int32 _Success;
    /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
    [DisplayName("成功")]
    [Description("成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数", "")]
    public Int32 Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private Int32 _Error;
    /// <summary>错误</summary>
    [DisplayName("错误")]
    [Description("错误")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Error", "错误", "")]
    public Int32 Error { get => _Error; set { if (OnPropertyChanging("Error", value)) { _Error = value; OnPropertyChanged("Error"); } } }

    private Int32 _Times;
    /// <summary>次数</summary>
    [DisplayName("次数")]
    [Description("次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Times", "次数", "")]
    public Int32 Times { get => _Times; set { if (OnPropertyChanging("Times", value)) { _Times = value; OnPropertyChanged("Times"); } } }

    private Int32 _Speed;
    /// <summary>速度。每秒处理数，执行端计算</summary>
    [DisplayName("速度")]
    [Description("速度。每秒处理数，执行端计算")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Speed", "速度。每秒处理数，执行端计算", "")]
    public Int32 Speed { get => _Speed; set { if (OnPropertyChanging("Speed", value)) { _Speed = value; OnPropertyChanged("Speed"); } } }

    private Int32 _Cost;
    /// <summary>耗时。秒，执行端计算的执行时间</summary>
    [DisplayName("耗时")]
    [Description("耗时。秒，执行端计算的执行时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Cost", "耗时。秒，执行端计算的执行时间", "", ItemType = "TimeSpan")]
    public Int32 Cost { get => _Cost; set { if (OnPropertyChanging("Cost", value)) { _Cost = value; OnPropertyChanged("Cost"); } } }

    private Int32 _FullCost;
    /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
    [DisplayName("全部耗时")]
    [Description("全部耗时。秒，从任务发放到执行完成的时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("FullCost", "全部耗时。秒，从任务发放到执行完成的时间", "", ItemType = "TimeSpan")]
    public Int32 FullCost { get => _FullCost; set { if (OnPropertyChanging("FullCost", value)) { _FullCost = value; OnPropertyChanged("FullCost"); } } }

    private JobStatus _Status;
    /// <summary>状态</summary>
    [DisplayName("状态")]
    [Description("状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Status", "状态", "")]
    public JobStatus Status { get => _Status; set { if (OnPropertyChanging("Status", value)) { _Status = value; OnPropertyChanged("Status"); } } }

    private Int32 _MsgCount;
    /// <summary>消息。消费消息数</summary>
    [DisplayName("消息")]
    [Description("消息。消费消息数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MsgCount", "消息。消费消息数", "")]
    public Int32 MsgCount { get => _MsgCount; set { if (OnPropertyChanging("MsgCount", value)) { _MsgCount = value; OnPropertyChanged("MsgCount"); } } }

    private String _Server;
    /// <summary>服务器</summary>
    [DisplayName("服务器")]
    [Description("服务器")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Server", "服务器", "")]
    public String Server { get => _Server; set { if (OnPropertyChanging("Server", value)) { _Server = value; OnPropertyChanged("Server"); } } }

    private Int32 _ProcessID;
    /// <summary>进程</summary>
    [DisplayName("进程")]
    [Description("进程")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ProcessID", "进程", "")]
    public Int32 ProcessID { get => _ProcessID; set { if (OnPropertyChanging("ProcessID", value)) { _ProcessID = value; OnPropertyChanged("ProcessID"); } } }

    private String _Key;
    /// <summary>最后键。Handler内记录作为样本的数据</summary>
    [DisplayName("最后键")]
    [Description("最后键。Handler内记录作为样本的数据")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Key", "最后键。Handler内记录作为样本的数据", "")]
    public String Key { get => _Key; set { if (OnPropertyChanging("Key", value)) { _Key = value; OnPropertyChanged("Key"); } } }

    private String _Data;
    /// <summary>数据。可以是Json数据，比如StatID</summary>
    [DisplayName("数据")]
    [Description("数据。可以是Json数据，比如StatID")]
    [DataObjectField(false, false, true, -1)]
    [BindColumn("Data", "数据。可以是Json数据，比如StatID", "")]
    public String Data { get => _Data; set { if (OnPropertyChanging("Data", value)) { _Data = value; OnPropertyChanged("Data"); } } }

    private String _Message;
    /// <summary>消息内容。Handler内记录的异常信息或其它任务消息</summary>
    [DisplayName("消息内容")]
    [Description("消息内容。Handler内记录的异常信息或其它任务消息")]
    [DataObjectField(false, false, true, -1)]
    [BindColumn("Message", "消息内容。Handler内记录的异常信息或其它任务消息", "")]
    public String Message { get => _Message; set { if (OnPropertyChanging("Message", value)) { _Message = value; OnPropertyChanged("Message"); } } }

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
            "ID" => _ID,
            "AppID" => _AppID,
            "JobID" => _JobID,
            "Client" => _Client,
            "DataTime" => _DataTime,
            "End" => _End,
            "BatchSize" => _BatchSize,
            "Total" => _Total,
            "Success" => _Success,
            "Error" => _Error,
            "Times" => _Times,
            "Speed" => _Speed,
            "Cost" => _Cost,
            "FullCost" => _FullCost,
            "Status" => _Status,
            "MsgCount" => _MsgCount,
            "Server" => _Server,
            "ProcessID" => _ProcessID,
            "Key" => _Key,
            "Data" => _Data,
            "Message" => _Message,
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
                case "ID": _ID = value.ToInt(); break;
                case "AppID": _AppID = value.ToInt(); break;
                case "JobID": _JobID = value.ToInt(); break;
                case "Client": _Client = Convert.ToString(value); break;
                case "DataTime": _DataTime = value.ToDateTime(); break;
                case "End": _End = value.ToDateTime(); break;
                case "BatchSize": _BatchSize = value.ToInt(); break;
                case "Total": _Total = value.ToInt(); break;
                case "Success": _Success = value.ToInt(); break;
                case "Error": _Error = value.ToInt(); break;
                case "Times": _Times = value.ToInt(); break;
                case "Speed": _Speed = value.ToInt(); break;
                case "Cost": _Cost = value.ToInt(); break;
                case "FullCost": _FullCost = value.ToInt(); break;
                case "Status": _Status = (JobStatus)value.ToInt(); break;
                case "MsgCount": _MsgCount = value.ToInt(); break;
                case "Server": _Server = Convert.ToString(value); break;
                case "ProcessID": _ProcessID = value.ToInt(); break;
                case "Key": _Key = Convert.ToString(value); break;
                case "Data": _Data = Convert.ToString(value); break;
                case "Message": _Message = Convert.ToString(value); break;
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
    public static JobTask FindByID(Int32 id)
    {
        if (id < 0) return null;

        return Find(_.ID == id);
    }

    /// <summary>根据作业查找</summary>
    /// <param name="jobId">作业</param>
    /// <returns>实体列表</returns>
    public static IList<JobTask> FindAllByJobID(Int32 jobId)
    {
        if (jobId < 0) return [];

        return FindAll(_.JobID == jobId);
    }

    /// <summary>根据数据时间查找</summary>
    /// <param name="dataTime">数据时间</param>
    /// <returns>实体列表</returns>
    public static IList<JobTask> FindAllByDataTime(DateTime dataTime)
    {
        if (dataTime.Year < 1000) return [];

        return FindAll(_.DataTime == dataTime);
    }
    #endregion

    #region 数据清理
    /// <summary>清理指定时间段内的数据</summary>
    /// <param name="start">开始时间。未指定时清理小于指定时间的所有数据</param>
    /// <param name="end">结束时间</param>
    /// <returns>清理行数</returns>
    public static Int32 DeleteWith(DateTime start, DateTime end)
    {
        if (start == end) return Delete(_.DataTime == start);

        return Delete(_.DataTime.Between(start, end));
    }
    #endregion

    #region 字段名
    /// <summary>取得作业任务字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>应用</summary>
        public static readonly Field AppID = FindByName("AppID");

        /// <summary>作业</summary>
        public static readonly Field JobID = FindByName("JobID");

        /// <summary>客户端。IP加进程</summary>
        public static readonly Field Client = FindByName("Client");

        /// <summary>数据时间。大于等于，定时调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
        public static readonly Field DataTime = FindByName("DataTime");

        /// <summary>结束。小于不等于，数据调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
        public static readonly Field End = FindByName("End");

        /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
        public static readonly Field BatchSize = FindByName("BatchSize");

        /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
        public static readonly Field Total = FindByName("Total");

        /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>错误</summary>
        public static readonly Field Error = FindByName("Error");

        /// <summary>次数</summary>
        public static readonly Field Times = FindByName("Times");

        /// <summary>速度。每秒处理数，执行端计算</summary>
        public static readonly Field Speed = FindByName("Speed");

        /// <summary>耗时。秒，执行端计算的执行时间</summary>
        public static readonly Field Cost = FindByName("Cost");

        /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
        public static readonly Field FullCost = FindByName("FullCost");

        /// <summary>状态</summary>
        public static readonly Field Status = FindByName("Status");

        /// <summary>消息。消费消息数</summary>
        public static readonly Field MsgCount = FindByName("MsgCount");

        /// <summary>服务器</summary>
        public static readonly Field Server = FindByName("Server");

        /// <summary>进程</summary>
        public static readonly Field ProcessID = FindByName("ProcessID");

        /// <summary>最后键。Handler内记录作为样本的数据</summary>
        public static readonly Field Key = FindByName("Key");

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        public static readonly Field Data = FindByName("Data");

        /// <summary>消息内容。Handler内记录的异常信息或其它任务消息</summary>
        public static readonly Field Message = FindByName("Message");

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

        /// <summary>数据时间。大于等于，定时调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
        public const String DataTime = "DataTime";

        /// <summary>结束。小于不等于，数据调度到达该时间点后触发（可能有偏移量），消息调度不适用</summary>
        public const String End = "End";

        /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
        public const String BatchSize = "BatchSize";

        /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
        public const String Total = "Total";

        /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
        public const String Success = "Success";

        /// <summary>错误</summary>
        public const String Error = "Error";

        /// <summary>次数</summary>
        public const String Times = "Times";

        /// <summary>速度。每秒处理数，执行端计算</summary>
        public const String Speed = "Speed";

        /// <summary>耗时。秒，执行端计算的执行时间</summary>
        public const String Cost = "Cost";

        /// <summary>全部耗时。秒，从任务发放到执行完成的时间</summary>
        public const String FullCost = "FullCost";

        /// <summary>状态</summary>
        public const String Status = "Status";

        /// <summary>消息。消费消息数</summary>
        public const String MsgCount = "MsgCount";

        /// <summary>服务器</summary>
        public const String Server = "Server";

        /// <summary>进程</summary>
        public const String ProcessID = "ProcessID";

        /// <summary>最后键。Handler内记录作为样本的数据</summary>
        public const String Key = "Key";

        /// <summary>数据。可以是Json数据，比如StatID</summary>
        public const String Data = "Data";

        /// <summary>消息内容。Handler内记录的异常信息或其它任务消息</summary>
        public const String Message = "Message";

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
