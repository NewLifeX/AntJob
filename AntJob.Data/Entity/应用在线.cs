using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace AntJob.Data.Entity
{
    /// <summary>应用在线。各应用多实例在线</summary>
    [Serializable]
    [DataObject]
    [Description("应用在线。各应用多实例在线")]
    [BindIndex("IU_AppOnline_Instance", true, "Instance")]
    [BindIndex("IX_AppOnline_Client", false, "Client")]
    [BindIndex("IX_AppOnline_AppID", false, "AppID")]
    [BindTable("AppOnline", Description = "应用在线。各应用多实例在线", ConnName = "Ant", DbType = DatabaseType.None)]
    public partial class AppOnline : IAppOnline
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

        private String _Instance;
        /// <summary>实例。IP加端口</summary>
        [DisplayName("实例")]
        [Description("实例。IP加端口")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Instance", "实例。IP加端口", "")]
        public String Instance { get { return _Instance; } set { if (OnPropertyChanging(__.Instance, value)) { _Instance = value; OnPropertyChanged(__.Instance); } } }

        private String _Client;
        /// <summary>客户端。IP加进程</summary>
        [DisplayName("客户端")]
        [Description("客户端。IP加进程")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Client", "客户端。IP加进程", "")]
        public String Client { get { return _Client; } set { if (OnPropertyChanging(__.Client, value)) { _Client = value; OnPropertyChanged(__.Client); } } }

        private String _Name;
        /// <summary>名称。机器名称</summary>
        [DisplayName("名称")]
        [Description("名称。机器名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Name", "名称。机器名称", "", Master = true)]
        public String Name { get { return _Name; } set { if (OnPropertyChanging(__.Name, value)) { _Name = value; OnPropertyChanged(__.Name); } } }

        private String _Version;
        /// <summary>版本。客户端</summary>
        [DisplayName("版本")]
        [Description("版本。客户端")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Version", "版本。客户端", "")]
        public String Version { get { return _Version; } set { if (OnPropertyChanging(__.Version, value)) { _Version = value; OnPropertyChanged(__.Version); } } }

        private String _Server;
        /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
        [DisplayName("服务端")]
        [Description("服务端。客户端登录到哪个服务端，IP加端口")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Server", "服务端。客户端登录到哪个服务端，IP加端口", "")]
        public String Server { get { return _Server; } set { if (OnPropertyChanging(__.Server, value)) { _Server = value; OnPropertyChanged(__.Server); } } }

        private Int32 _Tasks;
        /// <summary>任务数</summary>
        [DisplayName("任务数")]
        [Description("任务数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Tasks", "任务数", "")]
        public Int32 Tasks { get { return _Tasks; } set { if (OnPropertyChanging(__.Tasks, value)) { _Tasks = value; OnPropertyChanged(__.Tasks); } } }

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

        private Int64 _Error;
        /// <summary>错误</summary>
        [DisplayName("错误")]
        [Description("错误")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Error", "错误", "")]
        public Int64 Error { get { return _Error; } set { if (OnPropertyChanging(__.Error, value)) { _Error = value; OnPropertyChanged(__.Error); } } }

        private Int64 _Cost;
        /// <summary>耗时。总耗时，秒</summary>
        [DisplayName("耗时")]
        [Description("耗时。总耗时，秒")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Cost", "耗时。总耗时，秒", "")]
        public Int64 Cost { get { return _Cost; } set { if (OnPropertyChanging(__.Cost, value)) { _Cost = value; OnPropertyChanged(__.Cost); } } }

        private Int64 _Speed;
        /// <summary>速度。每秒处理数</summary>
        [DisplayName("速度")]
        [Description("速度。每秒处理数")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Speed", "速度。每秒处理数", "")]
        public Int64 Speed { get { return _Speed; } set { if (OnPropertyChanging(__.Speed, value)) { _Speed = value; OnPropertyChanged(__.Speed); } } }

        private String _LastKey;
        /// <summary>最后键</summary>
        [DisplayName("最后键")]
        [Description("最后键")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("LastKey", "最后键", "")]
        public String LastKey { get { return _LastKey; } set { if (OnPropertyChanging(__.LastKey, value)) { _LastKey = value; OnPropertyChanged(__.LastKey); } } }

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
                    case __.Instance : return _Instance;
                    case __.Client : return _Client;
                    case __.Name : return _Name;
                    case __.Version : return _Version;
                    case __.Server : return _Server;
                    case __.Tasks : return _Tasks;
                    case __.Total : return _Total;
                    case __.Success : return _Success;
                    case __.Error : return _Error;
                    case __.Cost : return _Cost;
                    case __.Speed : return _Speed;
                    case __.LastKey : return _LastKey;
                    case __.CreateTime : return _CreateTime;
                    case __.CreateIP : return _CreateIP;
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
                    case __.Instance : _Instance = Convert.ToString(value); break;
                    case __.Client : _Client = Convert.ToString(value); break;
                    case __.Name : _Name = Convert.ToString(value); break;
                    case __.Version : _Version = Convert.ToString(value); break;
                    case __.Server : _Server = Convert.ToString(value); break;
                    case __.Tasks : _Tasks = value.ToInt(); break;
                    case __.Total : _Total = value.ToLong(); break;
                    case __.Success : _Success = value.ToLong(); break;
                    case __.Error : _Error = value.ToLong(); break;
                    case __.Cost : _Cost = value.ToLong(); break;
                    case __.Speed : _Speed = value.ToLong(); break;
                    case __.LastKey : _LastKey = Convert.ToString(value); break;
                    case __.CreateTime : _CreateTime = value.ToDateTime(); break;
                    case __.CreateIP : _CreateIP = Convert.ToString(value); break;
                    case __.UpdateTime : _UpdateTime = value.ToDateTime(); break;
                    case __.UpdateIP : _UpdateIP = Convert.ToString(value); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得应用在线字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>应用</summary>
            public static readonly Field AppID = FindByName(__.AppID);

            /// <summary>实例。IP加端口</summary>
            public static readonly Field Instance = FindByName(__.Instance);

            /// <summary>客户端。IP加进程</summary>
            public static readonly Field Client = FindByName(__.Client);

            /// <summary>名称。机器名称</summary>
            public static readonly Field Name = FindByName(__.Name);

            /// <summary>版本。客户端</summary>
            public static readonly Field Version = FindByName(__.Version);

            /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
            public static readonly Field Server = FindByName(__.Server);

            /// <summary>任务数</summary>
            public static readonly Field Tasks = FindByName(__.Tasks);

            /// <summary>总数</summary>
            public static readonly Field Total = FindByName(__.Total);

            /// <summary>成功</summary>
            public static readonly Field Success = FindByName(__.Success);

            /// <summary>错误</summary>
            public static readonly Field Error = FindByName(__.Error);

            /// <summary>耗时。总耗时，秒</summary>
            public static readonly Field Cost = FindByName(__.Cost);

            /// <summary>速度。每秒处理数</summary>
            public static readonly Field Speed = FindByName(__.Speed);

            /// <summary>最后键</summary>
            public static readonly Field LastKey = FindByName(__.LastKey);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>创建地址</summary>
            public static readonly Field CreateIP = FindByName(__.CreateIP);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            /// <summary>更新地址</summary>
            public static readonly Field UpdateIP = FindByName(__.UpdateIP);

            static Field FindByName(String name) { return Meta.Table.FindByName(name); }
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

            /// <summary>版本。客户端</summary>
            public const String Version = "Version";

            /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
            public const String Server = "Server";

            /// <summary>任务数</summary>
            public const String Tasks = "Tasks";

            /// <summary>总数</summary>
            public const String Total = "Total";

            /// <summary>成功</summary>
            public const String Success = "Success";

            /// <summary>错误</summary>
            public const String Error = "Error";

            /// <summary>耗时。总耗时，秒</summary>
            public const String Cost = "Cost";

            /// <summary>速度。每秒处理数</summary>
            public const String Speed = "Speed";

            /// <summary>最后键</summary>
            public const String LastKey = "LastKey";

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

    /// <summary>应用在线。各应用多实例在线接口</summary>
    public partial interface IAppOnline
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>应用</summary>
        Int32 AppID { get; set; }

        /// <summary>实例。IP加端口</summary>
        String Instance { get; set; }

        /// <summary>客户端。IP加进程</summary>
        String Client { get; set; }

        /// <summary>名称。机器名称</summary>
        String Name { get; set; }

        /// <summary>版本。客户端</summary>
        String Version { get; set; }

        /// <summary>服务端。客户端登录到哪个服务端，IP加端口</summary>
        String Server { get; set; }

        /// <summary>任务数</summary>
        Int32 Tasks { get; set; }

        /// <summary>总数</summary>
        Int64 Total { get; set; }

        /// <summary>成功</summary>
        Int64 Success { get; set; }

        /// <summary>错误</summary>
        Int64 Error { get; set; }

        /// <summary>耗时。总耗时，秒</summary>
        Int64 Cost { get; set; }

        /// <summary>速度。每秒处理数</summary>
        Int64 Speed { get; set; }

        /// <summary>最后键</summary>
        String LastKey { get; set; }

        /// <summary>创建时间</summary>
        DateTime CreateTime { get; set; }

        /// <summary>创建地址</summary>
        String CreateIP { get; set; }

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