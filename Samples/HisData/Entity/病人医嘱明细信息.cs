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

/// <summary>病人医嘱明细信息</summary>
[Serializable]
[DataObject]
[Description("病人医嘱明细信息")]
[BindIndex("IU_ZYBHYZ1_DGROUPID_YZBM", true, "DGROUPID,YZBM")]
[BindIndex("IX_ZYBHYZ1_DGROUPID", false, "DGROUPID")]
[BindTable("ZYBHYZ1", Description = "病人医嘱明细信息", ConnName = "His", DbType = DatabaseType.None)]
public partial class ZYBHYZ1
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private Int32 _Dgroupid;
    /// <summary>医嘱组号</summary>
    [DisplayName("医嘱组号")]
    [Description("医嘱组号")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Dgroupid", "医嘱组号", "", Master = true)]
    public Int32 Dgroupid { get => _Dgroupid; set { if (OnPropertyChanging("Dgroupid", value)) { _Dgroupid = value; OnPropertyChanged("Dgroupid"); } } }

    private String _Yzbm;
    /// <summary>医嘱编码</summary>
    [DisplayName("医嘱编码")]
    [Description("医嘱编码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Yzbm", "医嘱编码", "")]
    public String Yzbm { get => _Yzbm; set { if (OnPropertyChanging("Yzbm", value)) { _Yzbm = value; OnPropertyChanged("Yzbm"); } } }

    private String _Yzmc;
    /// <summary>医嘱名称</summary>
    [DisplayName("医嘱名称")]
    [Description("医嘱名称")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Yzmc", "医嘱名称", "")]
    public String Yzmc { get => _Yzmc; set { if (OnPropertyChanging("Yzmc", value)) { _Yzmc = value; OnPropertyChanged("Yzmc"); } } }

    private Decimal _DJ;
    /// <summary>单价</summary>
    [DisplayName("单价")]
    [Description("单价")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DJ", "单价", "")]
    public Decimal DJ { get => _DJ; set { if (OnPropertyChanging("DJ", value)) { _DJ = value; OnPropertyChanged("DJ"); } } }

    private Double _SL;
    /// <summary>数量</summary>
    [DisplayName("数量")]
    [Description("数量")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("SL", "数量", "")]
    public Double SL { get => _SL; set { if (OnPropertyChanging("SL", value)) { _SL = value; OnPropertyChanged("SL"); } } }

    private Decimal _FY;
    /// <summary>费用</summary>
    [DisplayName("费用")]
    [Description("费用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("FY", "费用", "")]
    public Decimal FY { get => _FY; set { if (OnPropertyChanging("FY", value)) { _FY = value; OnPropertyChanged("FY"); } } }

    private Int32 _State;
    /// <summary>状态</summary>
    [DisplayName("状态")]
    [Description("状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("State", "状态", "")]
    public Int32 State { get => _State; set { if (OnPropertyChanging("State", value)) { _State = value; OnPropertyChanged("State"); } } }

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
            "Dgroupid" => _Dgroupid,
            "Yzbm" => _Yzbm,
            "Yzmc" => _Yzmc,
            "DJ" => _DJ,
            "SL" => _SL,
            "FY" => _FY,
            "State" => _State,
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
                case "Dgroupid": _Dgroupid = value.ToInt(); break;
                case "Yzbm": _Yzbm = Convert.ToString(value); break;
                case "Yzmc": _Yzmc = Convert.ToString(value); break;
                case "DJ": _DJ = Convert.ToDecimal(value); break;
                case "SL": _SL = value.ToDouble(); break;
                case "FY": _FY = Convert.ToDecimal(value); break;
                case "State": _State = value.ToInt(); break;
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

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="yzbm">医嘱编码</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<ZYBHYZ1> Search(String yzbm, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (!yzbm.IsNullOrEmpty()) exp &= _.Yzbm == yzbm;
        exp &= _.UpdateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得病人医嘱明细信息字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>医嘱组号</summary>
        public static readonly Field Dgroupid = FindByName("Dgroupid");

        /// <summary>医嘱编码</summary>
        public static readonly Field Yzbm = FindByName("Yzbm");

        /// <summary>医嘱名称</summary>
        public static readonly Field Yzmc = FindByName("Yzmc");

        /// <summary>单价</summary>
        public static readonly Field DJ = FindByName("DJ");

        /// <summary>数量</summary>
        public static readonly Field SL = FindByName("SL");

        /// <summary>费用</summary>
        public static readonly Field FY = FindByName("FY");

        /// <summary>状态</summary>
        public static readonly Field State = FindByName("State");

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

    /// <summary>取得病人医嘱明细信息字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>医嘱组号</summary>
        public const String Dgroupid = "Dgroupid";

        /// <summary>医嘱编码</summary>
        public const String Yzbm = "Yzbm";

        /// <summary>医嘱名称</summary>
        public const String Yzmc = "Yzmc";

        /// <summary>单价</summary>
        public const String DJ = "DJ";

        /// <summary>数量</summary>
        public const String SL = "SL";

        /// <summary>费用</summary>
        public const String FY = "FY";

        /// <summary>状态</summary>
        public const String State = "State";

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
