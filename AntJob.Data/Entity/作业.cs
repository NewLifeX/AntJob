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

/// <summary>作业。数据计算逻辑的主要单元，每个作业即是一个业务逻辑</summary>
[Serializable]
[DataObject]
[Description("作业。数据计算逻辑的主要单元，每个作业即是一个业务逻辑")]
[BindIndex("IU_Job_AppID_Name", true, "AppID,Name")]
[BindTable("Job", Description = "作业。数据计算逻辑的主要单元，每个作业即是一个业务逻辑", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class Job
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

    private String _Name;
    /// <summary>名称</summary>
    [DisplayName("名称")]
    [Description("名称")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("Name", "名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _ClassName;
    /// <summary>类名。支持该作业的处理器实现</summary>
    [DisplayName("类名")]
    [Description("类名。支持该作业的处理器实现")]
    [DataObjectField(false, false, true, 100)]
    [BindColumn("ClassName", "类名。支持该作业的处理器实现", "")]
    public String ClassName { get => _ClassName; set { if (OnPropertyChanging("ClassName", value)) { _ClassName = value; OnPropertyChanged("ClassName"); } } }

    private String _DisplayName;
    /// <summary>显示名</summary>
    [DisplayName("显示名")]
    [Description("显示名")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("DisplayName", "显示名", "")]
    public String DisplayName { get => _DisplayName; set { if (OnPropertyChanging("DisplayName", value)) { _DisplayName = value; OnPropertyChanged("DisplayName"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private JobModes _Mode;
    /// <summary>调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</summary>
    [DisplayName("调度模式")]
    [Description("调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Mode", "调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑", "")]
    public JobModes Mode { get => _Mode; set { if (OnPropertyChanging("Mode", value)) { _Mode = value; OnPropertyChanged("Mode"); } } }

    private String _Cron;
    /// <summary>执行频次。定时调度的Cron表达式</summary>
    [DisplayName("执行频次")]
    [Description("执行频次。定时调度的Cron表达式")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Cron", "执行频次。定时调度的Cron表达式", "")]
    public String Cron { get => _Cron; set { if (OnPropertyChanging("Cron", value)) { _Cron = value; OnPropertyChanged("Cron"); } } }

    private String _Topic;
    /// <summary>主题。消息调度时消费的主题</summary>
    [DisplayName("主题")]
    [Description("主题。消息调度时消费的主题")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Topic", "主题。消息调度时消费的主题", "")]
    public String Topic { get => _Topic; set { if (OnPropertyChanging("Topic", value)) { _Topic = value; OnPropertyChanged("Topic"); } } }

    private Int32 _MessageCount;
    /// <summary>消息数</summary>
    [DisplayName("消息数")]
    [Description("消息数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MessageCount", "消息数", "")]
    public Int32 MessageCount { get => _MessageCount; set { if (OnPropertyChanging("MessageCount", value)) { _MessageCount = value; OnPropertyChanged("MessageCount"); } } }

    private DateTime _DataTime;
    /// <summary>数据时间。下一次处理数据的时间，默认从当前时间开始</summary>
    [DisplayName("数据时间")]
    [Description("数据时间。下一次处理数据的时间，默认从当前时间开始")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("DataTime", "数据时间。下一次处理数据的时间，默认从当前时间开始", "")]
    public DateTime DataTime { get => _DataTime; set { if (OnPropertyChanging("DataTime", value)) { _DataTime = value; OnPropertyChanged("DataTime"); } } }

    private DateTime _End;
    /// <summary>结束。到该时间停止调度作业，默认不设置时永不停止</summary>
    [DisplayName("结束")]
    [Description("结束。到该时间停止调度作业，默认不设置时永不停止")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("End", "结束。到该时间停止调度作业，默认不设置时永不停止", "")]
    public DateTime End { get => _End; set { if (OnPropertyChanging("End", value)) { _End = value; OnPropertyChanged("End"); } } }

    private Int32 _Step;
    /// <summary>步进。切分任务的时间区间，秒</summary>
    [DisplayName("步进")]
    [Description("步进。切分任务的时间区间，秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Step", "步进。切分任务的时间区间，秒", "", ItemType = "TimeSpan")]
    public Int32 Step { get => _Step; set { if (OnPropertyChanging("Step", value)) { _Step = value; OnPropertyChanged("Step"); } } }

    private Int32 _BatchSize;
    /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
    [DisplayName("批大小")]
    [Description("批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("BatchSize", "批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用", "")]
    public Int32 BatchSize { get => _BatchSize; set { if (OnPropertyChanging("BatchSize", value)) { _BatchSize = value; OnPropertyChanged("BatchSize"); } } }

    private Int32 _Offset;
    /// <summary>偏移。距离AntServer当前时间的秒数，避免因服务器之间的时间误差而错过部分数据，秒</summary>
    [DisplayName("偏移")]
    [Description("偏移。距离AntServer当前时间的秒数，避免因服务器之间的时间误差而错过部分数据，秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Offset", "偏移。距离AntServer当前时间的秒数，避免因服务器之间的时间误差而错过部分数据，秒", "", ItemType = "TimeSpan")]
    public Int32 Offset { get => _Offset; set { if (OnPropertyChanging("Offset", value)) { _Offset = value; OnPropertyChanged("Offset"); } } }

    private Int32 _MaxTask;
    /// <summary>并行度。一共允许多少个任务并行处理，多执行端时平均分配，确保该作业整体并行度</summary>
    [Category("控制参数")]
    [DisplayName("并行度")]
    [Description("并行度。一共允许多少个任务并行处理，多执行端时平均分配，确保该作业整体并行度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxTask", "并行度。一共允许多少个任务并行处理，多执行端时平均分配，确保该作业整体并行度", "")]
    public Int32 MaxTask { get => _MaxTask; set { if (OnPropertyChanging("MaxTask", value)) { _MaxTask = value; OnPropertyChanged("MaxTask"); } } }

    private Int32 _MaxError;
    /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
    [Category("控制参数")]
    [DisplayName("最大错误")]
    [Description("最大错误。连续错误达到最大错误数时停止")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxError", "最大错误。连续错误达到最大错误数时停止", "")]
    public Int32 MaxError { get => _MaxError; set { if (OnPropertyChanging("MaxError", value)) { _MaxError = value; OnPropertyChanged("MaxError"); } } }

    private Int32 _MaxRetry;
    /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
    [Category("控制参数")]
    [DisplayName("最大重试")]
    [Description("最大重试。默认10次，超过该次数后将不再重试")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxRetry", "最大重试。默认10次，超过该次数后将不再重试", "")]
    public Int32 MaxRetry { get => _MaxRetry; set { if (OnPropertyChanging("MaxRetry", value)) { _MaxRetry = value; OnPropertyChanged("MaxRetry"); } } }

    private Int32 _MaxTime;
    /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
    [Category("控制参数")]
    [DisplayName("最大执行时间")]
    [Description("最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxTime", "最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器", "", ItemType = "TimeSpan")]
    public Int32 MaxTime { get => _MaxTime; set { if (OnPropertyChanging("MaxTime", value)) { _MaxTime = value; OnPropertyChanged("MaxTime"); } } }

    private Int32 _MaxRetain;
    /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认30天</summary>
    [Category("控制参数")]
    [DisplayName("保留")]
    [Description("保留。任务项保留天数，超过天数的任务项将被删除，默认30天")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxRetain", "保留。任务项保留天数，超过天数的任务项将被删除，默认30天", "")]
    public Int32 MaxRetain { get => _MaxRetain; set { if (OnPropertyChanging("MaxRetain", value)) { _MaxRetain = value; OnPropertyChanged("MaxRetain"); } } }

    private Int32 _MaxIdle;
    /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
    [Category("控制参数")]
    [DisplayName("最大空闲时间")]
    [Description("最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MaxIdle", "最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警", "", ItemType = "TimeSpan")]
    public Int32 MaxIdle { get => _MaxIdle; set { if (OnPropertyChanging("MaxIdle", value)) { _MaxIdle = value; OnPropertyChanged("MaxIdle"); } } }

    private Int32 _ErrorDelay;
    /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
    [Category("控制参数")]
    [DisplayName("错误延迟")]
    [Description("错误延迟。默认60秒，出错延迟后重新发放")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ErrorDelay", "错误延迟。默认60秒，出错延迟后重新发放", "", ItemType = "TimeSpan")]
    public Int32 ErrorDelay { get => _ErrorDelay; set { if (OnPropertyChanging("ErrorDelay", value)) { _ErrorDelay = value; OnPropertyChanged("ErrorDelay"); } } }

    private DateTime _Deadline;
    /// <summary>最后期限。超过该时间后，任务将不再执行</summary>
    [Category("控制参数")]
    [DisplayName("最后期限")]
    [Description("最后期限。超过该时间后，任务将不再执行")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("Deadline", "最后期限。超过该时间后，任务将不再执行", "")]
    public DateTime Deadline { get => _Deadline; set { if (OnPropertyChanging("Deadline", value)) { _Deadline = value; OnPropertyChanged("Deadline"); } } }

    private Int64 _Total;
    /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
    [Category("统计")]
    [DisplayName("总数")]
    [Description("总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Total", "总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1", "")]
    public Int64 Total { get => _Total; set { if (OnPropertyChanging("Total", value)) { _Total = value; OnPropertyChanged("Total"); } } }

    private Int64 _Success;
    /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
    [Category("统计")]
    [DisplayName("成功")]
    [Description("成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数", "")]
    public Int64 Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private Int32 _Error;
    /// <summary>错误</summary>
    [Category("统计")]
    [DisplayName("错误")]
    [Description("错误")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Error", "错误", "")]
    public Int32 Error { get => _Error; set { if (OnPropertyChanging("Error", value)) { _Error = value; OnPropertyChanged("Error"); } } }

    private Int32 _Times;
    /// <summary>次数</summary>
    [Category("统计")]
    [DisplayName("次数")]
    [Description("次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Times", "次数", "")]
    public Int32 Times { get => _Times; set { if (OnPropertyChanging("Times", value)) { _Times = value; OnPropertyChanged("Times"); } } }

    private Int32 _Speed;
    /// <summary>速度</summary>
    [Category("统计")]
    [DisplayName("速度")]
    [Description("速度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Speed", "速度", "")]
    public Int32 Speed { get => _Speed; set { if (OnPropertyChanging("Speed", value)) { _Speed = value; OnPropertyChanged("Speed"); } } }

    private JobStatus _LastStatus;
    /// <summary>最后状态。最后一次状态</summary>
    [Category("统计")]
    [DisplayName("最后状态")]
    [Description("最后状态。最后一次状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LastStatus", "最后状态。最后一次状态", "")]
    public JobStatus LastStatus { get => _LastStatus; set { if (OnPropertyChanging("LastStatus", value)) { _LastStatus = value; OnPropertyChanged("LastStatus"); } } }

    private DateTime _LastTime;
    /// <summary>最后时间。最后一次时间</summary>
    [Category("统计")]
    [DisplayName("最后时间")]
    [Description("最后时间。最后一次时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastTime", "最后时间。最后一次时间", "")]
    public DateTime LastTime { get => _LastTime; set { if (OnPropertyChanging("LastTime", value)) { _LastTime = value; OnPropertyChanged("LastTime"); } } }

    private String _Data;
    /// <summary>数据。Sql模板或C#模板</summary>
    [DisplayName("数据")]
    [Description("数据。Sql模板或C#模板")]
    [DataObjectField(false, false, true, -1)]
    [BindColumn("Data", "数据。Sql模板或C#模板", "")]
    public String Data { get => _Data; set { if (OnPropertyChanging("Data", value)) { _Data = value; OnPropertyChanged("Data"); } } }

    private Int32 _CreateUserID;
    /// <summary>创建人</summary>
    [Category("扩展")]
    [DisplayName("创建人")]
    [Description("创建人")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserID", "创建人", "")]
    public Int32 CreateUserID { get => _CreateUserID; set { if (OnPropertyChanging("CreateUserID", value)) { _CreateUserID = value; OnPropertyChanged("CreateUserID"); } } }

    private String _CreateUser;
    /// <summary>创建者</summary>
    [Category("扩展")]
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateUser", "创建者", "")]
    public String CreateUser { get => _CreateUser; set { if (OnPropertyChanging("CreateUser", value)) { _CreateUser = value; OnPropertyChanged("CreateUser"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [Category("扩展")]
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [Category("扩展")]
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private Int32 _UpdateUserID;
    /// <summary>更新人</summary>
    [Category("扩展")]
    [DisplayName("更新人")]
    [Description("更新人")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserID", "更新人", "")]
    public Int32 UpdateUserID { get => _UpdateUserID; set { if (OnPropertyChanging("UpdateUserID", value)) { _UpdateUserID = value; OnPropertyChanged("UpdateUserID"); } } }

    private String _UpdateUser;
    /// <summary>更新者</summary>
    [Category("扩展")]
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateUser", "更新者", "")]
    public String UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging("UpdateUser", value)) { _UpdateUser = value; OnPropertyChanged("UpdateUser"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [Category("扩展")]
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
    [Category("扩展")]
    [DisplayName("更新地址")]
    [Description("更新地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新地址", "")]
    public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private String _Remark;
    /// <summary>备注</summary>
    [Category("扩展")]
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 2000)]
    [BindColumn("Remark", "备注", "")]
    public String Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }
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
            "Name" => _Name,
            "ClassName" => _ClassName,
            "DisplayName" => _DisplayName,
            "Enable" => _Enable,
            "Mode" => _Mode,
            "Cron" => _Cron,
            "Topic" => _Topic,
            "MessageCount" => _MessageCount,
            "DataTime" => _DataTime,
            "End" => _End,
            "Step" => _Step,
            "BatchSize" => _BatchSize,
            "Offset" => _Offset,
            "MaxTask" => _MaxTask,
            "MaxError" => _MaxError,
            "MaxRetry" => _MaxRetry,
            "MaxTime" => _MaxTime,
            "MaxRetain" => _MaxRetain,
            "MaxIdle" => _MaxIdle,
            "ErrorDelay" => _ErrorDelay,
            "Deadline" => _Deadline,
            "Total" => _Total,
            "Success" => _Success,
            "Error" => _Error,
            "Times" => _Times,
            "Speed" => _Speed,
            "LastStatus" => _LastStatus,
            "LastTime" => _LastTime,
            "Data" => _Data,
            "CreateUserID" => _CreateUserID,
            "CreateUser" => _CreateUser,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateUserID" => _UpdateUserID,
            "UpdateUser" => _UpdateUser,
            "UpdateTime" => _UpdateTime,
            "UpdateIP" => _UpdateIP,
            "Remark" => _Remark,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToInt(); break;
                case "AppID": _AppID = value.ToInt(); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "ClassName": _ClassName = Convert.ToString(value); break;
                case "DisplayName": _DisplayName = Convert.ToString(value); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "Mode": _Mode = (JobModes)value.ToInt(); break;
                case "Cron": _Cron = Convert.ToString(value); break;
                case "Topic": _Topic = Convert.ToString(value); break;
                case "MessageCount": _MessageCount = value.ToInt(); break;
                case "DataTime": _DataTime = value.ToDateTime(); break;
                case "End": _End = value.ToDateTime(); break;
                case "Step": _Step = value.ToInt(); break;
                case "BatchSize": _BatchSize = value.ToInt(); break;
                case "Offset": _Offset = value.ToInt(); break;
                case "MaxTask": _MaxTask = value.ToInt(); break;
                case "MaxError": _MaxError = value.ToInt(); break;
                case "MaxRetry": _MaxRetry = value.ToInt(); break;
                case "MaxTime": _MaxTime = value.ToInt(); break;
                case "MaxRetain": _MaxRetain = value.ToInt(); break;
                case "MaxIdle": _MaxIdle = value.ToInt(); break;
                case "ErrorDelay": _ErrorDelay = value.ToInt(); break;
                case "Deadline": _Deadline = value.ToDateTime(); break;
                case "Total": _Total = value.ToLong(); break;
                case "Success": _Success = value.ToLong(); break;
                case "Error": _Error = value.ToInt(); break;
                case "Times": _Times = value.ToInt(); break;
                case "Speed": _Speed = value.ToInt(); break;
                case "LastStatus": _LastStatus = (JobStatus)value.ToInt(); break;
                case "LastTime": _LastTime = value.ToDateTime(); break;
                case "Data": _Data = Convert.ToString(value); break;
                case "CreateUserID": _CreateUserID = value.ToInt(); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateUserID": _UpdateUserID = value.ToInt(); break;
                case "UpdateUser": _UpdateUser = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "Remark": _Remark = Convert.ToString(value); break;
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

    #endregion

    #region 扩展查询
    #endregion

    #region 字段名
    /// <summary>取得作业字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>应用</summary>
        public static readonly Field AppID = FindByName("AppID");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>类名。支持该作业的处理器实现</summary>
        public static readonly Field ClassName = FindByName("ClassName");

        /// <summary>显示名</summary>
        public static readonly Field DisplayName = FindByName("DisplayName");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</summary>
        public static readonly Field Mode = FindByName("Mode");

        /// <summary>执行频次。定时调度的Cron表达式</summary>
        public static readonly Field Cron = FindByName("Cron");

        /// <summary>主题。消息调度时消费的主题</summary>
        public static readonly Field Topic = FindByName("Topic");

        /// <summary>消息数</summary>
        public static readonly Field MessageCount = FindByName("MessageCount");

        /// <summary>数据时间。下一次处理数据的时间，默认从当前时间开始</summary>
        public static readonly Field DataTime = FindByName("DataTime");

        /// <summary>结束。到该时间停止调度作业，默认不设置时永不停止</summary>
        public static readonly Field End = FindByName("End");

        /// <summary>步进。切分任务的时间区间，秒</summary>
        public static readonly Field Step = FindByName("Step");

        /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
        public static readonly Field BatchSize = FindByName("BatchSize");

        /// <summary>偏移。距离AntServer当前时间的秒数，避免因服务器之间的时间误差而错过部分数据，秒</summary>
        public static readonly Field Offset = FindByName("Offset");

        /// <summary>并行度。一共允许多少个任务并行处理，多执行端时平均分配，确保该作业整体并行度</summary>
        public static readonly Field MaxTask = FindByName("MaxTask");

        /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
        public static readonly Field MaxError = FindByName("MaxError");

        /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
        public static readonly Field MaxRetry = FindByName("MaxRetry");

        /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
        public static readonly Field MaxTime = FindByName("MaxTime");

        /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认30天</summary>
        public static readonly Field MaxRetain = FindByName("MaxRetain");

        /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
        public static readonly Field MaxIdle = FindByName("MaxIdle");

        /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
        public static readonly Field ErrorDelay = FindByName("ErrorDelay");

        /// <summary>最后期限。超过该时间后，任务将不再执行</summary>
        public static readonly Field Deadline = FindByName("Deadline");

        /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
        public static readonly Field Total = FindByName("Total");

        /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>错误</summary>
        public static readonly Field Error = FindByName("Error");

        /// <summary>次数</summary>
        public static readonly Field Times = FindByName("Times");

        /// <summary>速度</summary>
        public static readonly Field Speed = FindByName("Speed");

        /// <summary>最后状态。最后一次状态</summary>
        public static readonly Field LastStatus = FindByName("LastStatus");

        /// <summary>最后时间。最后一次时间</summary>
        public static readonly Field LastTime = FindByName("LastTime");

        /// <summary>数据。Sql模板或C#模板</summary>
        public static readonly Field Data = FindByName("Data");

        /// <summary>创建人</summary>
        public static readonly Field CreateUserID = FindByName("CreateUserID");

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新人</summary>
        public static readonly Field UpdateUserID = FindByName("UpdateUserID");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUser = FindByName("UpdateUser");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得作业字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>应用</summary>
        public const String AppID = "AppID";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>类名。支持该作业的处理器实现</summary>
        public const String ClassName = "ClassName";

        /// <summary>显示名</summary>
        public const String DisplayName = "DisplayName";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>调度模式。定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑</summary>
        public const String Mode = "Mode";

        /// <summary>执行频次。定时调度的Cron表达式</summary>
        public const String Cron = "Cron";

        /// <summary>主题。消息调度时消费的主题</summary>
        public const String Topic = "Topic";

        /// <summary>消息数</summary>
        public const String MessageCount = "MessageCount";

        /// <summary>数据时间。下一次处理数据的时间，默认从当前时间开始</summary>
        public const String DataTime = "DataTime";

        /// <summary>结束。到该时间停止调度作业，默认不设置时永不停止</summary>
        public const String End = "End";

        /// <summary>步进。切分任务的时间区间，秒</summary>
        public const String Step = "Step";

        /// <summary>批大小。数据调度每次抽取数据的分页大小，或消息调度每次处理的消息数，定时调度不适用</summary>
        public const String BatchSize = "BatchSize";

        /// <summary>偏移。距离AntServer当前时间的秒数，避免因服务器之间的时间误差而错过部分数据，秒</summary>
        public const String Offset = "Offset";

        /// <summary>并行度。一共允许多少个任务并行处理，多执行端时平均分配，确保该作业整体并行度</summary>
        public const String MaxTask = "MaxTask";

        /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
        public const String MaxError = "MaxError";

        /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
        public const String MaxRetry = "MaxRetry";

        /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
        public const String MaxTime = "MaxTime";

        /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认30天</summary>
        public const String MaxRetain = "MaxRetain";

        /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
        public const String MaxIdle = "MaxIdle";

        /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
        public const String ErrorDelay = "ErrorDelay";

        /// <summary>最后期限。超过该时间后，任务将不再执行</summary>
        public const String Deadline = "Deadline";

        /// <summary>总数。任务处理的总数据，例如数据调度抽取得到的总行数，定时调度默认1</summary>
        public const String Total = "Total";

        /// <summary>成功。成功处理的数据，取自于Handler.Execute返回值，或者ProcessItem返回true的个数</summary>
        public const String Success = "Success";

        /// <summary>错误</summary>
        public const String Error = "Error";

        /// <summary>次数</summary>
        public const String Times = "Times";

        /// <summary>速度</summary>
        public const String Speed = "Speed";

        /// <summary>最后状态。最后一次状态</summary>
        public const String LastStatus = "LastStatus";

        /// <summary>最后时间。最后一次时间</summary>
        public const String LastTime = "LastTime";

        /// <summary>数据。Sql模板或C#模板</summary>
        public const String Data = "Data";

        /// <summary>创建人</summary>
        public const String CreateUserID = "CreateUserID";

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新人</summary>
        public const String UpdateUserID = "UpdateUserID";

        /// <summary>更新者</summary>
        public const String UpdateUser = "UpdateUser";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}
