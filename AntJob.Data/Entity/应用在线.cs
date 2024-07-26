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

/// <summary>应用在线。各个数据计算应用多实例在线</summary>
[Serializable]
[DataObject]
[Description("应用在线。各个数据计算应用多实例在线")]
[BindIndex("IU_AppOnline_Instance", true, "Instance")]
[BindIndex("IX_AppOnline_Client", false, "Client")]
[BindIndex("IX_AppOnline_AppID", false, "AppID")]
[BindTable("AppOnline", Description = "应用在线。各个数据计算应用多实例在线", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class AppOnline
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

    private String _Instance;
    /// <summary>实例。IP加端口</summary>
    [DisplayName("实例")]
    [Description("实例。IP加端口")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Instance", "实例。IP加端口", "")]
    public String Instance { get => _Instance; set { if (OnPropertyChanging("Instance", value)) { _Instance = value; OnPropertyChanged("Instance"); } } }

    private String _Client;
    /// <summary>客户端。IP加进程</summary>
    [DisplayName("客户端")]
    [Description("客户端。IP加进程")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Client", "客户端。IP加进程", "")]
    public String Client { get => _Client; set { if (OnPropertyChanging("Client", value)) { _Client = value; OnPropertyChanged("Client"); } } }

    private String _Name;
    /// <summary>名称。机器名称</summary>
    [DisplayName("名称")]
    [Description("名称。机器名称")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Name", "名称。机器名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private Int32 _ProcessId;
    /// <summary>进程。进程Id</summary>
    [DisplayName("进程")]
    [Description("进程。进程Id")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ProcessId", "进程。进程Id", "")]
    public Int32 ProcessId { get => _ProcessId; set { if (OnPropertyChanging("ProcessId", value)) { _ProcessId = value; OnPropertyChanged("ProcessId"); } } }

    private String _Version;
    /// <summary>版本。客户端</summary>
    [DisplayName("版本")]
    [Description("版本。客户端")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Version", "版本。客户端", "")]
    public String Version { get => _Version; set { if (OnPropertyChanging("Version", value)) { _Version = value; OnPropertyChanged("Version"); } } }

    private DateTime _CompileTime;
    /// <summary>编译时间</summary>
    [DisplayName("编译时间")]
    [Description("编译时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CompileTime", "编译时间", "")]
    public DateTime CompileTime { get => _CompileTime; set { if (OnPropertyChanging("CompileTime", value)) { _CompileTime = value; OnPropertyChanged("CompileTime"); } } }

    private String _Server;
    /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
    [DisplayName("服务端")]
    [Description("服务端。客户端登录到哪个服务端，IP加端口")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Server", "服务端。客户端登录到哪个服务端，IP加端口", "")]
    public String Server { get => _Server; set { if (OnPropertyChanging("Server", value)) { _Server = value; OnPropertyChanged("Server"); } } }

    private Boolean _Enable;
    /// <summary>启用。是否允许申请任务</summary>
    [DisplayName("启用")]
    [Description("启用。是否允许申请任务")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用。是否允许申请任务", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Int32 _Tasks;
    /// <summary>任务数</summary>
    [DisplayName("任务数")]
    [Description("任务数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Tasks", "任务数", "")]
    public Int32 Tasks { get => _Tasks; set { if (OnPropertyChanging("Tasks", value)) { _Tasks = value; OnPropertyChanged("Tasks"); } } }

    private Int64 _Total;
    /// <summary>总数</summary>
    [DisplayName("总数")]
    [Description("总数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Total", "总数", "")]
    public Int64 Total { get => _Total; set { if (OnPropertyChanging("Total", value)) { _Total = value; OnPropertyChanged("Total"); } } }

    private Int64 _Success;
    /// <summary>成功</summary>
    [DisplayName("成功")]
    [Description("成功")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Success", "成功", "")]
    public Int64 Success { get => _Success; set { if (OnPropertyChanging("Success", value)) { _Success = value; OnPropertyChanged("Success"); } } }

    private Int64 _Error;
    /// <summary>错误</summary>
    [DisplayName("错误")]
    [Description("错误")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Error", "错误", "")]
    public Int64 Error { get => _Error; set { if (OnPropertyChanging("Error", value)) { _Error = value; OnPropertyChanged("Error"); } } }

    private Int64 _Cost;
    /// <summary>耗时。执行任务总耗时，秒</summary>
    [DisplayName("耗时")]
    [Description("耗时。执行任务总耗时，秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Cost", "耗时。执行任务总耗时，秒", "", ItemType = "TimeSpan")]
    public Int64 Cost { get => _Cost; set { if (OnPropertyChanging("Cost", value)) { _Cost = value; OnPropertyChanged("Cost"); } } }

    private Int64 _Speed;
    /// <summary>速度。每秒处理数</summary>
    [DisplayName("速度")]
    [Description("速度。每秒处理数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Speed", "速度。每秒处理数", "")]
    public Int64 Speed { get => _Speed; set { if (OnPropertyChanging("Speed", value)) { _Speed = value; OnPropertyChanged("Speed"); } } }

    private String _LastKey;
    /// <summary>最后键</summary>
    [DisplayName("最后键")]
    [Description("最后键")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("LastKey", "最后键", "")]
    public String LastKey { get => _LastKey; set { if (OnPropertyChanging("LastKey", value)) { _LastKey = value; OnPropertyChanged("LastKey"); } } }

    private String _TraceId;
    /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
    [Category("扩展")]
    [DisplayName("追踪")]
    [Description("追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("TraceId", "追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链", "")]
    public String TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }

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
            "Instance" => _Instance,
            "Client" => _Client,
            "Name" => _Name,
            "ProcessId" => _ProcessId,
            "Version" => _Version,
            "CompileTime" => _CompileTime,
            "Server" => _Server,
            "Enable" => _Enable,
            "Tasks" => _Tasks,
            "Total" => _Total,
            "Success" => _Success,
            "Error" => _Error,
            "Cost" => _Cost,
            "Speed" => _Speed,
            "LastKey" => _LastKey,
            "TraceId" => _TraceId,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateTime" => _UpdateTime,
            "UpdateIP" => _UpdateIP,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToInt(); break;
                case "AppID": _AppID = value.ToInt(); break;
                case "Instance": _Instance = Convert.ToString(value); break;
                case "Client": _Client = Convert.ToString(value); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "ProcessId": _ProcessId = value.ToInt(); break;
                case "Version": _Version = Convert.ToString(value); break;
                case "CompileTime": _CompileTime = value.ToDateTime(); break;
                case "Server": _Server = Convert.ToString(value); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "Tasks": _Tasks = value.ToInt(); break;
                case "Total": _Total = value.ToLong(); break;
                case "Success": _Success = value.ToLong(); break;
                case "Error": _Error = value.ToLong(); break;
                case "Cost": _Cost = value.ToLong(); break;
                case "Speed": _Speed = value.ToLong(); break;
                case "LastKey": _LastKey = Convert.ToString(value); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
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
    /// <summary>取得应用在线字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>应用</summary>
        public static readonly Field AppID = FindByName("AppID");

        /// <summary>实例。IP加端口</summary>
        public static readonly Field Instance = FindByName("Instance");

        /// <summary>客户端。IP加进程</summary>
        public static readonly Field Client = FindByName("Client");

        /// <summary>名称。机器名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>进程。进程Id</summary>
        public static readonly Field ProcessId = FindByName("ProcessId");

        /// <summary>版本。客户端</summary>
        public static readonly Field Version = FindByName("Version");

        /// <summary>编译时间</summary>
        public static readonly Field CompileTime = FindByName("CompileTime");

        /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
        public static readonly Field Server = FindByName("Server");

        /// <summary>启用。是否允许申请任务</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>任务数</summary>
        public static readonly Field Tasks = FindByName("Tasks");

        /// <summary>总数</summary>
        public static readonly Field Total = FindByName("Total");

        /// <summary>成功</summary>
        public static readonly Field Success = FindByName("Success");

        /// <summary>错误</summary>
        public static readonly Field Error = FindByName("Error");

        /// <summary>耗时。执行任务总耗时，秒</summary>
        public static readonly Field Cost = FindByName("Cost");

        /// <summary>速度。每秒处理数</summary>
        public static readonly Field Speed = FindByName("Speed");

        /// <summary>最后键</summary>
        public static readonly Field LastKey = FindByName("LastKey");

        /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得应用在线字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>应用</summary>
        public const String AppID = "AppID";

        /// <summary>实例。IP加端口</summary>
        public const String Instance = "Instance";

        /// <summary>客户端。IP加进程</summary>
        public const String Client = "Client";

        /// <summary>名称。机器名称</summary>
        public const String Name = "Name";

        /// <summary>进程。进程Id</summary>
        public const String ProcessId = "ProcessId";

        /// <summary>版本。客户端</summary>
        public const String Version = "Version";

        /// <summary>编译时间</summary>
        public const String CompileTime = "CompileTime";

        /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
        public const String Server = "Server";

        /// <summary>启用。是否允许申请任务</summary>
        public const String Enable = "Enable";

        /// <summary>任务数</summary>
        public const String Tasks = "Tasks";

        /// <summary>总数</summary>
        public const String Total = "Total";

        /// <summary>成功</summary>
        public const String Success = "Success";

        /// <summary>错误</summary>
        public const String Error = "Error";

        /// <summary>耗时。执行任务总耗时，秒</summary>
        public const String Cost = "Cost";

        /// <summary>速度。每秒处理数</summary>
        public const String Speed = "Speed";

        /// <summary>最后键</summary>
        public const String LastKey = "LastKey";

        /// <summary>追踪。链路追踪，用于APM性能追踪定位，还原该事件的调用链</summary>
        public const String TraceId = "TraceId";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";
    }
    #endregion
}
