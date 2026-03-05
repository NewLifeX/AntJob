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

namespace HisData;

/// <summary>病人基本信息</summary>
[Serializable]
[DataObject]
[Description("病人基本信息")]
[BindIndex("IU_ZYBH0_BHID", true, "BHID")]
[BindIndex("IX_ZYBH0_CreateTime", false, "CreateTime")]
[BindTable("ZYBH0", Description = "病人基本信息", ConnName = "His", DbType = DatabaseType.None)]
public partial class ZYBH0
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private Int32 _Bhid;
    /// <summary>病人ID</summary>
    [DisplayName("病人ID")]
    [Description("病人ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Bhid", "病人ID", "", Master = true)]
    public Int32 Bhid { get => _Bhid; set { if (OnPropertyChanging("Bhid", value)) { _Bhid = value; OnPropertyChanged("Bhid"); } } }

    private String _XM;
    /// <summary>姓名</summary>
    [DisplayName("姓名")]
    [Description("姓名")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("XM", "姓名", "")]
    public String XM { get => _XM; set { if (OnPropertyChanging("XM", value)) { _XM = value; OnPropertyChanged("XM"); } } }

    private DateTime _Ryrq;
    /// <summary>入院日期</summary>
    [DisplayName("入院日期")]
    [Description("入院日期")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("Ryrq", "入院日期", "")]
    public DateTime Ryrq { get => _Ryrq; set { if (OnPropertyChanging("Ryrq", value)) { _Ryrq = value; OnPropertyChanged("Ryrq"); } } }

    private DateTime _Cyrq;
    /// <summary>出院日期</summary>
    [DisplayName("出院日期")]
    [Description("出院日期")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("Cyrq", "出院日期", "")]
    public DateTime Cyrq { get => _Cyrq; set { if (OnPropertyChanging("Cyrq", value)) { _Cyrq = value; OnPropertyChanged("Cyrq"); } } }

    private String _Sfzh;
    /// <summary>身份证号</summary>
    [DisplayName("身份证号")]
    [Description("身份证号")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Sfzh", "身份证号", "")]
    public String Sfzh { get => _Sfzh; set { if (OnPropertyChanging("Sfzh", value)) { _Sfzh = value; OnPropertyChanged("Sfzh"); } } }

    private String _FB;
    /// <summary>费用类别</summary>
    [DisplayName("费用类别")]
    [Description("费用类别")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("FB", "费用类别", "")]
    public String FB { get => _FB; set { if (OnPropertyChanging("FB", value)) { _FB = value; OnPropertyChanged("FB"); } } }

    private Int32 _State;
    /// <summary>状态</summary>
    [DisplayName("状态")]
    [Description("状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("State", "状态", "")]
    public Int32 State { get => _State; set { if (OnPropertyChanging("State", value)) { _State = value; OnPropertyChanged("State"); } } }

    private Int32 _Flag;
    /// <summary>标记</summary>
    [DisplayName("标记")]
    [Description("标记")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Flag", "标记", "")]
    public Int32 Flag { get => _Flag; set { if (OnPropertyChanging("Flag", value)) { _Flag = value; OnPropertyChanged("Flag"); } } }

    private String _Remark;
    /// <summary>内容</summary>
    [DisplayName("内容")]
    [Description("内容")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "内容", "")]
    public String Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }

    private String _CreateUser;
    /// <summary>创建者</summary>
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateUser", "创建者", "")]
    public String CreateUser { get => _CreateUser; set { if (OnPropertyChanging("CreateUser", value)) { _CreateUser = value; OnPropertyChanged("CreateUser"); } } }

    private Int32 _CreateUserID;
    /// <summary>创建者</summary>
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserID", "创建者", "")]
    public Int32 CreateUserID { get => _CreateUserID; set { if (OnPropertyChanging("CreateUserID", value)) { _CreateUserID = value; OnPropertyChanged("CreateUserID"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private String _UpdateUser;
    /// <summary>更新者</summary>
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateUser", "更新者", "")]
    public String UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging("UpdateUser", value)) { _UpdateUser = value; OnPropertyChanged("UpdateUser"); } } }

    private Int32 _UpdateUserID;
    /// <summary>更新者</summary>
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserID", "更新者", "")]
    public Int32 UpdateUserID { get => _UpdateUserID; set { if (OnPropertyChanging("UpdateUserID", value)) { _UpdateUserID = value; OnPropertyChanged("UpdateUserID"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
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
            "Bhid" => _Bhid,
            "XM" => _XM,
            "Ryrq" => _Ryrq,
            "Cyrq" => _Cyrq,
            "Sfzh" => _Sfzh,
            "FB" => _FB,
            "State" => _State,
            "Flag" => _Flag,
            "Remark" => _Remark,
            "CreateUser" => _CreateUser,
            "CreateUserID" => _CreateUserID,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateUser" => _UpdateUser,
            "UpdateUserID" => _UpdateUserID,
            "UpdateTime" => _UpdateTime,
            "UpdateIP" => _UpdateIP,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "ID": _ID = value.ToInt(); break;
                case "Bhid": _Bhid = value.ToInt(); break;
                case "XM": _XM = Convert.ToString(value); break;
                case "Ryrq": _Ryrq = value.ToDateTime(); break;
                case "Cyrq": _Cyrq = value.ToDateTime(); break;
                case "Sfzh": _Sfzh = Convert.ToString(value); break;
                case "FB": _FB = Convert.ToString(value); break;
                case "State": _State = value.ToInt(); break;
                case "Flag": _Flag = value.ToInt(); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                case "CreateUser": _CreateUser = Convert.ToString(value); break;
                case "CreateUserID": _CreateUserID = value.ToInt(); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateUser": _UpdateUser = Convert.ToString(value); break;
                case "UpdateUserID": _UpdateUserID = value.ToInt(); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    #endregion

    #region 扩展查询
    #endregion

    #region 字段名
    /// <summary>取得病人基本信息字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>病人ID</summary>
        public static readonly Field Bhid = FindByName("Bhid");

        /// <summary>姓名</summary>
        public static readonly Field XM = FindByName("XM");

        /// <summary>入院日期</summary>
        public static readonly Field Ryrq = FindByName("Ryrq");

        /// <summary>出院日期</summary>
        public static readonly Field Cyrq = FindByName("Cyrq");

        /// <summary>身份证号</summary>
        public static readonly Field Sfzh = FindByName("Sfzh");

        /// <summary>费用类别</summary>
        public static readonly Field FB = FindByName("FB");

        /// <summary>状态</summary>
        public static readonly Field State = FindByName("State");

        /// <summary>标记</summary>
        public static readonly Field Flag = FindByName("Flag");

        /// <summary>内容</summary>
        public static readonly Field Remark = FindByName("Remark");

        /// <summary>创建者</summary>
        public static readonly Field CreateUser = FindByName("CreateUser");

        /// <summary>创建者</summary>
        public static readonly Field CreateUserID = FindByName("CreateUserID");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUser = FindByName("UpdateUser");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUserID = FindByName("UpdateUserID");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得病人基本信息字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>病人ID</summary>
        public const String Bhid = "Bhid";

        /// <summary>姓名</summary>
        public const String XM = "XM";

        /// <summary>入院日期</summary>
        public const String Ryrq = "Ryrq";

        /// <summary>出院日期</summary>
        public const String Cyrq = "Cyrq";

        /// <summary>身份证号</summary>
        public const String Sfzh = "Sfzh";

        /// <summary>费用类别</summary>
        public const String FB = "FB";

        /// <summary>状态</summary>
        public const String State = "State";

        /// <summary>标记</summary>
        public const String Flag = "Flag";

        /// <summary>内容</summary>
        public const String Remark = "Remark";

        /// <summary>创建者</summary>
        public const String CreateUser = "CreateUser";

        /// <summary>创建者</summary>
        public const String CreateUserID = "CreateUserID";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新者</summary>
        public const String UpdateUser = "UpdateUser";

        /// <summary>更新者</summary>
        public const String UpdateUserID = "UpdateUserID";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";
    }
    #endregion
}
