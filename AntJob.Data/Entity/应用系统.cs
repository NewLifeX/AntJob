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

/// <summary>应用系统。管理数据计算作业的应用模块，计算作业隶属于某个应用</summary>
[Serializable]
[DataObject]
[Description("应用系统。管理数据计算作业的应用模块，计算作业隶属于某个应用")]
[BindIndex("IU_App_Name", true, "Name")]
[BindTable("App", Description = "应用系统。管理数据计算作业的应用模块，计算作业隶属于某个应用", ConnName = "Ant", DbType = DatabaseType.None)]
public partial class App
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private String _Name;
    /// <summary>名称。应用英文名</summary>
    [DisplayName("名称")]
    [Description("名称。应用英文名")]
    [DataObjectField(false, false, false, 50)]
    [BindColumn("Name", "名称。应用英文名", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _DisplayName;
    /// <summary>显示名。应用中文名</summary>
    [DisplayName("显示名")]
    [Description("显示名。应用中文名")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("DisplayName", "显示名。应用中文名", "")]
    public String DisplayName { get => _DisplayName; set { if (OnPropertyChanging("DisplayName", value)) { _DisplayName = value; OnPropertyChanged("DisplayName"); } } }

    private String _Secret;
    /// <summary>密钥。一般不设置，应用默认接入</summary>
    [DisplayName("密钥")]
    [Description("密钥。一般不设置，应用默认接入")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Secret", "密钥。一般不设置，应用默认接入", "")]
    public String Secret { get => _Secret; set { if (OnPropertyChanging("Secret", value)) { _Secret = value; OnPropertyChanged("Secret"); } } }

    private String _Category;
    /// <summary>类别</summary>
    [DisplayName("类别")]
    [Description("类别")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Category", "类别", "")]
    public String Category { get => _Category; set { if (OnPropertyChanging("Category", value)) { _Category = value; OnPropertyChanged("Category"); } } }

    private String _Version;
    /// <summary>版本</summary>
    [DisplayName("版本")]
    [Description("版本")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Version", "版本", "")]
    public String Version { get => _Version; set { if (OnPropertyChanging("Version", value)) { _Version = value; OnPropertyChanged("Version"); } } }

    private DateTime _CompileTime;
    /// <summary>编译时间</summary>
    [DisplayName("编译时间")]
    [Description("编译时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CompileTime", "编译时间", "")]
    public DateTime CompileTime { get => _CompileTime; set { if (OnPropertyChanging("CompileTime", value)) { _CompileTime = value; OnPropertyChanged("CompileTime"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Int32 _JobCount;
    /// <summary>作业数。该应用下作业个数</summary>
    [DisplayName("作业数")]
    [Description("作业数。该应用下作业个数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("JobCount", "作业数。该应用下作业个数", "")]
    public Int32 JobCount { get => _JobCount; set { if (OnPropertyChanging("JobCount", value)) { _JobCount = value; OnPropertyChanged("JobCount"); } } }

    private Int32 _MessageCount;
    /// <summary>消息数。该应用下消息条数</summary>
    [DisplayName("消息数")]
    [Description("消息数。该应用下消息条数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("MessageCount", "消息数。该应用下消息条数", "")]
    public Int32 MessageCount { get => _MessageCount; set { if (OnPropertyChanging("MessageCount", value)) { _MessageCount = value; OnPropertyChanged("MessageCount"); } } }

    private Int32 _ManagerId;
    /// <summary>管理人。负责该应用的管理员</summary>
    [DisplayName("管理人")]
    [Description("管理人。负责该应用的管理员")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ManagerId", "管理人。负责该应用的管理员", "")]
    public Int32 ManagerId { get => _ManagerId; set { if (OnPropertyChanging("ManagerId", value)) { _ManagerId = value; OnPropertyChanged("ManagerId"); } } }

    private String _Manager;
    /// <summary>管理者</summary>
    [DisplayName("管理者")]
    [Description("管理者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Manager", "管理者", "")]
    public String Manager { get => _Manager; set { if (OnPropertyChanging("Manager", value)) { _Manager = value; OnPropertyChanged("Manager"); } } }

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
    [DataObjectField(false, false, true, 500)]
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
            "Name" => _Name,
            "DisplayName" => _DisplayName,
            "Secret" => _Secret,
            "Category" => _Category,
            "Version" => _Version,
            "CompileTime" => _CompileTime,
            "Enable" => _Enable,
            "JobCount" => _JobCount,
            "MessageCount" => _MessageCount,
            "ManagerId" => _ManagerId,
            "Manager" => _Manager,
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
                case "Name": _Name = Convert.ToString(value); break;
                case "DisplayName": _DisplayName = Convert.ToString(value); break;
                case "Secret": _Secret = Convert.ToString(value); break;
                case "Category": _Category = Convert.ToString(value); break;
                case "Version": _Version = Convert.ToString(value); break;
                case "CompileTime": _CompileTime = value.ToDateTime(); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "JobCount": _JobCount = value.ToInt(); break;
                case "MessageCount": _MessageCount = value.ToInt(); break;
                case "ManagerId": _ManagerId = value.ToInt(); break;
                case "Manager": _Manager = Convert.ToString(value); break;
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
    /// <summary>管理人</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public XCode.Membership.User MyManager => Extends.Get(nameof(MyManager), k => XCode.Membership.User.FindByID(ManagerId));

    /// <summary>管理人</summary>
    [Map(nameof(ManagerId), typeof(XCode.Membership.User), "ID")]
    public String ManagerName => MyManager?.Name;

    #endregion

    #region 扩展查询
    #endregion

    #region 字段名
    /// <summary>取得应用系统字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>名称。应用英文名</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>显示名。应用中文名</summary>
        public static readonly Field DisplayName = FindByName("DisplayName");

        /// <summary>密钥。一般不设置，应用默认接入</summary>
        public static readonly Field Secret = FindByName("Secret");

        /// <summary>类别</summary>
        public static readonly Field Category = FindByName("Category");

        /// <summary>版本</summary>
        public static readonly Field Version = FindByName("Version");

        /// <summary>编译时间</summary>
        public static readonly Field CompileTime = FindByName("CompileTime");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>作业数。该应用下作业个数</summary>
        public static readonly Field JobCount = FindByName("JobCount");

        /// <summary>消息数。该应用下消息条数</summary>
        public static readonly Field MessageCount = FindByName("MessageCount");

        /// <summary>管理人。负责该应用的管理员</summary>
        public static readonly Field ManagerId = FindByName("ManagerId");

        /// <summary>管理者</summary>
        public static readonly Field Manager = FindByName("Manager");

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

    /// <summary>取得应用系统字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>名称。应用英文名</summary>
        public const String Name = "Name";

        /// <summary>显示名。应用中文名</summary>
        public const String DisplayName = "DisplayName";

        /// <summary>密钥。一般不设置，应用默认接入</summary>
        public const String Secret = "Secret";

        /// <summary>类别</summary>
        public const String Category = "Category";

        /// <summary>版本</summary>
        public const String Version = "Version";

        /// <summary>编译时间</summary>
        public const String CompileTime = "CompileTime";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>作业数。该应用下作业个数</summary>
        public const String JobCount = "JobCount";

        /// <summary>消息数。该应用下消息条数</summary>
        public const String MessageCount = "MessageCount";

        /// <summary>管理人。负责该应用的管理员</summary>
        public const String ManagerId = "ManagerId";

        /// <summary>管理者</summary>
        public const String Manager = "Manager";

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
