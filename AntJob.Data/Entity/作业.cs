using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity
{
    /// <summary>作业</summary>
    [Serializable]
    [DataObject]
    [Description("作业")]
    [BindIndex("IU_Job_AppID_Name", true, "AppID,Name")]
    [BindTable("Job", Description = "作业", ConnName = "Ant", DbType = DatabaseType.None)]
    public partial class Job : IJob
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

        private String _Name;
        /// <summary>名称</summary>
        [DisplayName("名称")]
        [Description("名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Name", "名称", "", Master = true)]
        public String Name { get { return _Name; } set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } } }

        private String _ClassName;
        /// <summary>类名。支持该作业的处理器实现</summary>
        [DisplayName("类名")]
        [Description("类名。支持该作业的处理器实现")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("ClassName", "类名。支持该作业的处理器实现", "")]
        public String ClassName { get { return _ClassName; } set { if (OnPropertyChanging(__.ClassName, value)) { _ClassName = value; OnPropertyChanged(__.ClassName); } } }

        private String _DisplayName;
        /// <summary>显示名</summary>
        [DisplayName("显示名")]
        [Description("显示名")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("DisplayName", "显示名", "")]
        public String DisplayName { get { return _DisplayName; } set { if (OnPropertyChanging(__.DisplayName, value)) { _DisplayName = value; OnPropertyChanged(__.DisplayName); } } }

        private JobModes _Mode;
        /// <summary>调度模式</summary>
        [DisplayName("调度模式")]
        [Description("调度模式")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Mode", "调度模式", "")]
        public JobModes Mode { get { return _Mode; } set { if (OnPropertyChanging(__.Mode, value)) { _Mode = value; OnPropertyChanged(__.Mode); } } }

        private String _Topic;
        /// <summary>主题。消息调度时消费的主题</summary>
        [DisplayName("主题")]
        [Description("主题。消息调度时消费的主题")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Topic", "主题。消息调度时消费的主题", "")]
        public String Topic { get { return _Topic; } set { if (OnPropertyChanging(__.Topic, value)) { _Topic = value; OnPropertyChanged(__.Topic); } } }

        private Int32 _MessageCount;
        /// <summary>消息数</summary>
        [DisplayName("消息数")]
        [Description("消息数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MessageCount", "消息数", "")]
        public Int32 MessageCount { get { return _MessageCount; } set { if (OnPropertyChanging(__.MessageCount, value)) { _MessageCount = value; OnPropertyChanged(__.MessageCount); } } }

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

        private Int32 _Step;
        /// <summary>步进。最大区间大小，秒</summary>
        [DisplayName("步进")]
        [Description("步进。最大区间大小，秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Step", "步进。最大区间大小，秒", "")]
        public Int32 Step { get { return _Step; } set { if (OnPropertyChanging(__.Step, value)) { _Step = value; OnPropertyChanged(__.Step); } } }

        private Int32 _MinStep;
        /// <summary>最小步进。默认5秒</summary>
        [DisplayName("最小步进")]
        [Description("最小步进。默认5秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MinStep", "最小步进。默认5秒", "")]
        public Int32 MinStep { get { return _MinStep; } set { if (OnPropertyChanging(__.MinStep, value)) { _MinStep = value; OnPropertyChanged(__.MinStep); } } }

        private Int32 _MaxStep;
        /// <summary>最大步进。默认3600秒</summary>
        [DisplayName("最大步进")]
        [Description("最大步进。默认3600秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxStep", "最大步进。默认3600秒", "")]
        public Int32 MaxStep { get { return _MaxStep; } set { if (OnPropertyChanging(__.MaxStep, value)) { _MaxStep = value; OnPropertyChanged(__.MaxStep); } } }

        private Int32 _StepRate;
        /// <summary>步进率。动态调节步进时，不能超过该比率，百分位，默认100%</summary>
        [DisplayName("步进率")]
        [Description("步进率。动态调节步进时，不能超过该比率，百分位，默认100%")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("StepRate", "步进率。动态调节步进时，不能超过该比率，百分位，默认100%", "")]
        public Int32 StepRate { get { return _StepRate; } set { if (OnPropertyChanging(__.StepRate, value)) { _StepRate = value; OnPropertyChanged(__.StepRate); } } }

        private Int32 _BatchSize;
        /// <summary>批大小</summary>
        [DisplayName("批大小")]
        [Description("批大小")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("BatchSize", "批大小", "")]
        public Int32 BatchSize { get { return _BatchSize; } set { if (OnPropertyChanging(__.BatchSize, value)) { _BatchSize = value; OnPropertyChanged(__.BatchSize); } } }

        private Int32 _Offset;
        /// <summary>偏移。距离实时时间的秒数，部分业务不能跑到实时，秒</summary>
        [DisplayName("偏移")]
        [Description("偏移。距离实时时间的秒数，部分业务不能跑到实时，秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Offset", "偏移。距离实时时间的秒数，部分业务不能跑到实时，秒", "")]
        public Int32 Offset { get { return _Offset; } set { if (OnPropertyChanging(__.Offset, value)) { _Offset = value; OnPropertyChanged(__.Offset); } } }

        private Int32 _MaxTask;
        /// <summary>并行。多任务并行处理</summary>
        [DisplayName("并行")]
        [Description("并行。多任务并行处理")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxTask", "并行。多任务并行处理", "")]
        public Int32 MaxTask { get { return _MaxTask; } set { if (OnPropertyChanging(__.MaxTask, value)) { _MaxTask = value; OnPropertyChanged(__.MaxTask); } } }

        private Int32 _MaxError;
        /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
        [DisplayName("最大错误")]
        [Description("最大错误。连续错误达到最大错误数时停止")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxError", "最大错误。连续错误达到最大错误数时停止", "")]
        public Int32 MaxError { get { return _MaxError; } set { if (OnPropertyChanging(__.MaxError, value)) { _MaxError = value; OnPropertyChanged(__.MaxError); } } }

        private Int32 _MaxRetry;
        /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
        [DisplayName("最大重试")]
        [Description("最大重试。默认10次，超过该次数后将不再重试")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxRetry", "最大重试。默认10次，超过该次数后将不再重试", "")]
        public Int32 MaxRetry { get { return _MaxRetry; } set { if (OnPropertyChanging(__.MaxRetry, value)) { _MaxRetry = value; OnPropertyChanged(__.MaxRetry); } } }

        private Int32 _MaxTime;
        /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
        [DisplayName("最大执行时间")]
        [Description("最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxTime", "最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器", "")]
        public Int32 MaxTime { get { return _MaxTime; } set { if (OnPropertyChanging(__.MaxTime, value)) { _MaxTime = value; OnPropertyChanged(__.MaxTime); } } }

        private Int32 _MaxRetain;
        /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认3天</summary>
        [DisplayName("保留")]
        [Description("保留。任务项保留天数，超过天数的任务项将被删除，默认3天")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxRetain", "保留。任务项保留天数，超过天数的任务项将被删除，默认3天", "")]
        public Int32 MaxRetain { get { return _MaxRetain; } set { if (OnPropertyChanging(__.MaxRetain, value)) { _MaxRetain = value; OnPropertyChanged(__.MaxRetain); } } }

        private Int32 _MaxIdle;
        /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
        [DisplayName("最大空闲时间")]
        [Description("最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MaxIdle", "最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警", "")]
        public Int32 MaxIdle { get { return _MaxIdle; } set { if (OnPropertyChanging(__.MaxIdle, value)) { _MaxIdle = value; OnPropertyChanged(__.MaxIdle); } } }

        private Int32 _ErrorDelay;
        /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
        [DisplayName("错误延迟")]
        [Description("错误延迟。默认60秒，出错延迟后重新发放")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ErrorDelay", "错误延迟。默认60秒，出错延迟后重新发放", "")]
        public Int32 ErrorDelay { get { return _ErrorDelay; } set { if (OnPropertyChanging(__.ErrorDelay, value)) { _ErrorDelay = value; OnPropertyChanged(__.ErrorDelay); } } }

        private Int64 _Total;
        /// <summary>总数</summary>
        [DisplayName("总数")]
        [Description("总数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Total", "总数", "")]
        public Int64 Total { get { return _Total; } set { if (OnPropertyChanging(__.Total, value)) { _Total = value; OnPropertyChanged(__.Total); } } }

        private Int64 _Success;
        /// <summary>成功</summary>
        [DisplayName("成功")]
        [Description("成功")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Success", "成功", "")]
        public Int64 Success { get { return _Success; } set { if (OnPropertyChanging(__.Success, value)) { _Success = value; OnPropertyChanged(__.Success); } } }

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

        private Boolean _Enable;
        /// <summary>启用</summary>
        [DisplayName("启用")]
        [Description("启用")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Enable", "启用", "")]
        public Boolean Enable { get { return _Enable; } set { if (OnPropertyChanging(__.Enable, value)) { _Enable = value; OnPropertyChanged(__.Enable); } } }

        private String _Description;
        /// <summary>内容</summary>
        [DisplayName("内容")]
        [Description("内容")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Message", "内容", "")]
        public String Description { get { return _Description; } set { if (OnPropertyChanging(__.Description, value)) { _Description = value; OnPropertyChanged(__.Description); } } }

        private Int32 _CreateUserID;
        /// <summary>创建者</summary>
        [DisplayName("创建者")]
        [Description("创建者")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("CreateUserID", "创建者", "")]
        public Int32 CreateUserID { get { return _CreateUserID; } set { if (OnPropertyChanging(__.CreateUserID, value)) { _CreateUserID = value; OnPropertyChanged(__.CreateUserID); } } }

        private String _CreateUser;
        /// <summary>创建者</summary>
        [DisplayName("创建者")]
        [Description("创建者")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("CreateUser", "创建者", "")]
        public String CreateUser { get { return _CreateUser; } set { if (OnPropertyChanging(__.CreateUser, value)) { _CreateUser = value; OnPropertyChanged(__.CreateUser); } } }

        private DateTime _CreateTime;
        /// <summary>创建时间</summary>
        [DisplayName("创建时间")]
        [Description("创建时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("CreateTime", "创建时间", "")]
        public DateTime CreateTime { get { return _CreateTime; } set { if (OnPropertyChanging(__.CreateTime, value)) { _CreateTime = value; OnPropertyChanged(__.CreateTime); } } }

        private String _CreateIP;
        /// <summary>创建地址</summary>
        [DisplayName("创建地址")]
        [Description("创建地址")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("CreateIP", "创建地址", "")]
        public String CreateIP { get { return _CreateIP; } set { if (OnPropertyChanging(__.CreateIP, value)) { _CreateIP = value; OnPropertyChanged(__.CreateIP); } } }

        private Int32 _UpdateUserID;
        /// <summary>更新者</summary>
        [DisplayName("更新者")]
        [Description("更新者")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("UpdateUserID", "更新者", "")]
        public Int32 UpdateUserID { get { return _UpdateUserID; } set { if (OnPropertyChanging(__.UpdateUserID, value)) { _UpdateUserID = value; OnPropertyChanged(__.UpdateUserID); } } }

        private String _UpdateUser;
        /// <summary>更新者</summary>
        [DisplayName("更新者")]
        [Description("更新者")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("UpdateUser", "更新者", "")]
        public String UpdateUser { get { return _UpdateUser; } set { if (OnPropertyChanging(__.UpdateUser, value)) { _UpdateUser = value; OnPropertyChanged(__.UpdateUser); } } }

        private DateTime _UpdateTime;
        /// <summary>更新时间</summary>
        [DisplayName("更新时间")]
        [Description("更新时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("UpdateTime", "更新时间", "")]
        public DateTime UpdateTime { get { return _UpdateTime; } set { if (OnPropertyChanging(__.UpdateTime, value)) { _UpdateTime = value; OnPropertyChanged(__.UpdateTime); } } }

        private String _UpdateIP;
        /// <summary>更新地址</summary>
        [DisplayName("更新地址")]
        [Description("更新地址")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("UpdateIP", "更新地址", "")]
        public String UpdateIP { get { return _UpdateIP; } set { if (OnPropertyChanging(__.UpdateIP, value)) { _UpdateIP = value; OnPropertyChanged(__.UpdateIP); } } }
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
                    case __.Name : return _Name;
                    case __.ClassName : return _ClassName;
                    case __.DisplayName : return _DisplayName;
                    case __.Mode : return _Mode;
                    case __.Topic : return _Topic;
                    case __.MessageCount : return _MessageCount;
                    case __.Start : return _Start;
                    case __.End : return _End;
                    case __.Step : return _Step;
                    case __.MinStep : return _MinStep;
                    case __.MaxStep : return _MaxStep;
                    case __.StepRate : return _StepRate;
                    case __.BatchSize : return _BatchSize;
                    case __.Offset : return _Offset;
                    case __.MaxTask : return _MaxTask;
                    case __.MaxError : return _MaxError;
                    case __.MaxRetry : return _MaxRetry;
                    case __.MaxTime : return _MaxTime;
                    case __.MaxRetain : return _MaxRetain;
                    case __.MaxIdle : return _MaxIdle;
                    case __.ErrorDelay : return _ErrorDelay;
                    case __.Total : return _Total;
                    case __.Success : return _Success;
                    case __.Error : return _Error;
                    case __.Times : return _Times;
                    case __.Speed : return _Speed;
                    case __.Enable : return _Enable;
                    case __.Description : return _Description;
                    case __.CreateUserID : return _CreateUserID;
                    case __.CreateUser : return _CreateUser;
                    case __.CreateTime : return _CreateTime;
                    case __.CreateIP : return _CreateIP;
                    case __.UpdateUserID : return _UpdateUserID;
                    case __.UpdateUser : return _UpdateUser;
                    case __.UpdateTime : return _UpdateTime;
                    case __.UpdateIP : return _UpdateIP;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID : _ID = value.ToInt(); break;
                    case __.AppID : _AppID = value.ToInt(); break;
                    case __.Name : _Name = Convert.ToString(value); break;
                    case __.ClassName : _ClassName = Convert.ToString(value); break;
                    case __.DisplayName : _DisplayName = Convert.ToString(value); break;
                    case __.Mode : _Mode = (JobModes)value.ToInt(); break;
                    case __.Topic : _Topic = Convert.ToString(value); break;
                    case __.MessageCount : _MessageCount = value.ToInt(); break;
                    case __.Start : _Start = value.ToDateTime(); break;
                    case __.End : _End = value.ToDateTime(); break;
                    case __.Step : _Step = value.ToInt(); break;
                    case __.MinStep : _MinStep = value.ToInt(); break;
                    case __.MaxStep : _MaxStep = value.ToInt(); break;
                    case __.StepRate : _StepRate = value.ToInt(); break;
                    case __.BatchSize : _BatchSize = value.ToInt(); break;
                    case __.Offset : _Offset = value.ToInt(); break;
                    case __.MaxTask : _MaxTask = value.ToInt(); break;
                    case __.MaxError : _MaxError = value.ToInt(); break;
                    case __.MaxRetry : _MaxRetry = value.ToInt(); break;
                    case __.MaxTime : _MaxTime = value.ToInt(); break;
                    case __.MaxRetain : _MaxRetain = value.ToInt(); break;
                    case __.MaxIdle : _MaxIdle = value.ToInt(); break;
                    case __.ErrorDelay : _ErrorDelay = value.ToInt(); break;
                    case __.Total : _Total = value.ToLong(); break;
                    case __.Success : _Success = value.ToLong(); break;
                    case __.Error : _Error = value.ToInt(); break;
                    case __.Times : _Times = value.ToInt(); break;
                    case __.Speed : _Speed = value.ToInt(); break;
                    case __.Enable : _Enable = value.ToBoolean(); break;
                    case __.Description : _Description = Convert.ToString(value); break;
                    case __.CreateUserID : _CreateUserID = value.ToInt(); break;
                    case __.CreateUser : _CreateUser = Convert.ToString(value); break;
                    case __.CreateTime : _CreateTime = value.ToDateTime(); break;
                    case __.CreateIP : _CreateIP = Convert.ToString(value); break;
                    case __.UpdateUserID : _UpdateUserID = value.ToInt(); break;
                    case __.UpdateUser : _UpdateUser = Convert.ToString(value); break;
                    case __.UpdateTime : _UpdateTime = value.ToDateTime(); break;
                    case __.UpdateIP : _UpdateIP = Convert.ToString(value); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得作业字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>应用</summary>
            public static readonly Field AppID = FindByName(__.AppID);

            /// <summary>名称</summary>
            public static readonly Field Name = FindByName(__.Name);

            /// <summary>类名。支持该作业的处理器实现</summary>
            public static readonly Field ClassName = FindByName(__.ClassName);

            /// <summary>显示名</summary>
            public static readonly Field DisplayName = FindByName(__.DisplayName);

            /// <summary>调度模式</summary>
            public static readonly Field Mode = FindByName(__.Mode);

            /// <summary>主题。消息调度时消费的主题</summary>
            public static readonly Field Topic = FindByName(__.Topic);

            /// <summary>消息数</summary>
            public static readonly Field MessageCount = FindByName(__.MessageCount);

            /// <summary>开始。大于等于</summary>
            public static readonly Field Start = FindByName(__.Start);

            /// <summary>结束。小于，不等于</summary>
            public static readonly Field End = FindByName(__.End);

            /// <summary>步进。最大区间大小，秒</summary>
            public static readonly Field Step = FindByName(__.Step);

            /// <summary>最小步进。默认5秒</summary>
            public static readonly Field MinStep = FindByName(__.MinStep);

            /// <summary>最大步进。默认3600秒</summary>
            public static readonly Field MaxStep = FindByName(__.MaxStep);

            /// <summary>步进率。动态调节步进时，不能超过该比率，百分位，默认100%</summary>
            public static readonly Field StepRate = FindByName(__.StepRate);

            /// <summary>批大小</summary>
            public static readonly Field BatchSize = FindByName(__.BatchSize);

            /// <summary>偏移。距离实时时间的秒数，部分业务不能跑到实时，秒</summary>
            public static readonly Field Offset = FindByName(__.Offset);

            /// <summary>并行。多任务并行处理</summary>
            public static readonly Field MaxTask = FindByName(__.MaxTask);

            /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
            public static readonly Field MaxError = FindByName(__.MaxError);

            /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
            public static readonly Field MaxRetry = FindByName(__.MaxRetry);

            /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
            public static readonly Field MaxTime = FindByName(__.MaxTime);

            /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认3天</summary>
            public static readonly Field MaxRetain = FindByName(__.MaxRetain);

            /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
            public static readonly Field MaxIdle = FindByName(__.MaxIdle);

            /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
            public static readonly Field ErrorDelay = FindByName(__.ErrorDelay);

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

            /// <summary>启用</summary>
            public static readonly Field Enable = FindByName(__.Enable);

            /// <summary>内容</summary>
            public static readonly Field Description = FindByName(__.Description);

            /// <summary>创建者</summary>
            public static readonly Field CreateUserID = FindByName(__.CreateUserID);

            /// <summary>创建者</summary>
            public static readonly Field CreateUser = FindByName(__.CreateUser);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>创建地址</summary>
            public static readonly Field CreateIP = FindByName(__.CreateIP);

            /// <summary>更新者</summary>
            public static readonly Field UpdateUserID = FindByName(__.UpdateUserID);

            /// <summary>更新者</summary>
            public static readonly Field UpdateUser = FindByName(__.UpdateUser);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            /// <summary>更新地址</summary>
            public static readonly Field UpdateIP = FindByName(__.UpdateIP);

            static Field FindByName(String name) { return Meta.Table.FindByName(name); }
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

            /// <summary>调度模式</summary>
            public const String Mode = "Mode";

            /// <summary>主题。消息调度时消费的主题</summary>
            public const String Topic = "Topic";

            /// <summary>消息数</summary>
            public const String MessageCount = "MessageCount";

            /// <summary>开始。大于等于</summary>
            public const String Start = "Start";

            /// <summary>结束。小于，不等于</summary>
            public const String End = "End";

            /// <summary>步进。最大区间大小，秒</summary>
            public const String Step = "Step";

            /// <summary>最小步进。默认5秒</summary>
            public const String MinStep = "MinStep";

            /// <summary>最大步进。默认3600秒</summary>
            public const String MaxStep = "MaxStep";

            /// <summary>步进率。动态调节步进时，不能超过该比率，百分位，默认100%</summary>
            public const String StepRate = "StepRate";

            /// <summary>批大小</summary>
            public const String BatchSize = "BatchSize";

            /// <summary>偏移。距离实时时间的秒数，部分业务不能跑到实时，秒</summary>
            public const String Offset = "Offset";

            /// <summary>并行。多任务并行处理</summary>
            public const String MaxTask = "MaxTask";

            /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
            public const String MaxError = "MaxError";

            /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
            public const String MaxRetry = "MaxRetry";

            /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
            public const String MaxTime = "MaxTime";

            /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认3天</summary>
            public const String MaxRetain = "MaxRetain";

            /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
            public const String MaxIdle = "MaxIdle";

            /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
            public const String ErrorDelay = "ErrorDelay";

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

            /// <summary>启用</summary>
            public const String Enable = "Enable";

            /// <summary>内容</summary>
            public const String Description = "Description";

            /// <summary>创建者</summary>
            public const String CreateUserID = "CreateUserID";

            /// <summary>创建者</summary>
            public const String CreateUser = "CreateUser";

            /// <summary>创建时间</summary>
            public const String CreateTime = "CreateTime";

            /// <summary>创建地址</summary>
            public const String CreateIP = "CreateIP";

            /// <summary>更新者</summary>
            public const String UpdateUserID = "UpdateUserID";

            /// <summary>更新者</summary>
            public const String UpdateUser = "UpdateUser";

            /// <summary>更新时间</summary>
            public const String UpdateTime = "UpdateTime";

            /// <summary>更新地址</summary>
            public const String UpdateIP = "UpdateIP";
        }
        #endregion
    }

    /// <summary>作业接口</summary>
    public partial interface IJob
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>应用</summary>
        Int32 AppID { get; set; }

        /// <summary>名称</summary>
        String Name { get; set; }

        /// <summary>类名。支持该作业的处理器实现</summary>
        String ClassName { get; set; }

        /// <summary>显示名</summary>
        String DisplayName { get; set; }

        /// <summary>调度模式</summary>
        JobModes Mode { get; set; }

        /// <summary>主题。消息调度时消费的主题</summary>
        String Topic { get; set; }

        /// <summary>消息数</summary>
        Int32 MessageCount { get; set; }

        /// <summary>开始。大于等于</summary>
        DateTime Start { get; set; }

        /// <summary>结束。小于，不等于</summary>
        DateTime End { get; set; }

        /// <summary>步进。最大区间大小，秒</summary>
        Int32 Step { get; set; }

        /// <summary>最小步进。默认5秒</summary>
        Int32 MinStep { get; set; }

        /// <summary>最大步进。默认3600秒</summary>
        Int32 MaxStep { get; set; }

        /// <summary>步进率。动态调节步进时，不能超过该比率，百分位，默认100%</summary>
        Int32 StepRate { get; set; }

        /// <summary>批大小</summary>
        Int32 BatchSize { get; set; }

        /// <summary>偏移。距离实时时间的秒数，部分业务不能跑到实时，秒</summary>
        Int32 Offset { get; set; }

        /// <summary>并行。多任务并行处理</summary>
        Int32 MaxTask { get; set; }

        /// <summary>最大错误。连续错误达到最大错误数时停止</summary>
        Int32 MaxError { get; set; }

        /// <summary>最大重试。默认10次，超过该次数后将不再重试</summary>
        Int32 MaxRetry { get; set; }

        /// <summary>最大执行时间。默认600秒，超过该时间则认为执行器故障，将会把该任务分配给其它执行器</summary>
        Int32 MaxTime { get; set; }

        /// <summary>保留。任务项保留天数，超过天数的任务项将被删除，默认3天</summary>
        Int32 MaxRetain { get; set; }

        /// <summary>最大空闲时间。默认3600秒，超过该时间不更新则认为应用程序故障，系统触发告警</summary>
        Int32 MaxIdle { get; set; }

        /// <summary>错误延迟。默认60秒，出错延迟后重新发放</summary>
        Int32 ErrorDelay { get; set; }

        /// <summary>总数</summary>
        Int64 Total { get; set; }

        /// <summary>成功</summary>
        Int64 Success { get; set; }

        /// <summary>错误</summary>
        Int32 Error { get; set; }

        /// <summary>次数</summary>
        Int32 Times { get; set; }

        /// <summary>速度</summary>
        Int32 Speed { get; set; }

        /// <summary>启用</summary>
        Boolean Enable { get; set; }

        /// <summary>内容</summary>
        String Description { get; set; }

        /// <summary>创建者</summary>
        Int32 CreateUserID { get; set; }

        /// <summary>创建者</summary>
        String CreateUser { get; set; }

        /// <summary>创建时间</summary>
        DateTime CreateTime { get; set; }

        /// <summary>创建地址</summary>
        String CreateIP { get; set; }

        /// <summary>更新者</summary>
        Int32 UpdateUserID { get; set; }

        /// <summary>更新者</summary>
        String UpdateUser { get; set; }

        /// <summary>更新时间</summary>
        DateTime UpdateTime { get; set; }

        /// <summary>更新地址</summary>
        String UpdateIP { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}