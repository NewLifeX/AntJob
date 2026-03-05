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

/// <summary>病人药房请领单分月表202001</summary>
[Serializable]
[DataObject]
[Description("病人药房请领单分月表202001")]
[BindIndex("IX_ZYYFQLD_BHID", false, "BHID")]
[BindTable("ZYYFQLD", Description = "病人药房请领单分月表202001", ConnName = "His", DbType = DatabaseType.None)]
public partial class ZYYFQLD
{
    #region 属性
    private Int32 _ID;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("ID", "编号", "")]
    public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

    private Int32 _Qlrq;
    /// <summary>请领日期</summary>
    [DisplayName("请领日期")]
    [Description("请领日期")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Qlrq", "请领日期", "")]
    public Int32 Qlrq { get => _Qlrq; set { if (OnPropertyChanging("Qlrq", value)) { _Qlrq = value; OnPropertyChanged("Qlrq"); } } }

    private Int32 _Qlsj;
    /// <summary>请领时间</summary>
    [DisplayName("请领时间")]
    [Description("请领时间")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Qlsj", "请领时间", "")]
    public Int32 Qlsj { get => _Qlsj; set { if (OnPropertyChanging("Qlsj", value)) { _Qlsj = value; OnPropertyChanged("Qlsj"); } } }

    private String _Ksbm;
    /// <summary>请领科室</summary>
    [DisplayName("请领科室")]
    [Description("请领科室")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Ksbm", "请领科室", "")]
    public String Ksbm { get => _Ksbm; set { if (OnPropertyChanging("Ksbm", value)) { _Ksbm = value; OnPropertyChanged("Ksbm"); } } }

    private Int32 _Yzgroupid;
    /// <summary>医嘱ID</summary>
    [DisplayName("医嘱ID")]
    [Description("医嘱ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Yzgroupid", "医嘱ID", "")]
    public Int32 Yzgroupid { get => _Yzgroupid; set { if (OnPropertyChanging("Yzgroupid", value)) { _Yzgroupid = value; OnPropertyChanged("Yzgroupid"); } } }

    private Int32 _Bhid;
    /// <summary>病人ID</summary>
    [DisplayName("病人ID")]
    [Description("病人ID")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Bhid", "病人ID", "")]
    public Int32 Bhid { get => _Bhid; set { if (OnPropertyChanging("Bhid", value)) { _Bhid = value; OnPropertyChanged("Bhid"); } } }

    private String _Yzbm;
    /// <summary>药品编码</summary>
    [DisplayName("药品编码")]
    [Description("药品编码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Yzbm", "药品编码", "")]
    public String Yzbm { get => _Yzbm; set { if (OnPropertyChanging("Yzbm", value)) { _Yzbm = value; OnPropertyChanged("Yzbm"); } } }

    private Decimal _DJ;
    /// <summary>单价</summary>
    [DisplayName("单价")]
    [Description("单价")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DJ", "单价", "")]
    public Decimal DJ { get => _DJ; set { if (OnPropertyChanging("DJ", value)) { _DJ = value; OnPropertyChanged("DJ"); } } }

    private Double _SL;
    /// <summary>请领数量</summary>
    [DisplayName("请领数量")]
    [Description("请领数量")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("SL", "请领数量", "")]
    public Double SL { get => _SL; set { if (OnPropertyChanging("SL", value)) { _SL = value; OnPropertyChanged("SL"); } } }

    private String _Yfbm;
    /// <summary>发药药房</summary>
    [DisplayName("发药药房")]
    [Description("发药药房")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Yfbm", "发药药房", "")]
    public String Yfbm { get => _Yfbm; set { if (OnPropertyChanging("Yfbm", value)) { _Yfbm = value; OnPropertyChanged("Yfbm"); } } }

    private Int32 _Fyrq;
    /// <summary>发药日期</summary>
    [DisplayName("发药日期")]
    [Description("发药日期")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Fyrq", "发药日期", "")]
    public Int32 Fyrq { get => _Fyrq; set { if (OnPropertyChanging("Fyrq", value)) { _Fyrq = value; OnPropertyChanged("Fyrq"); } } }

    private Int32 _State;
    /// <summary>状态</summary>
    [DisplayName("状态")]
    [Description("状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("State", "状态", "")]
    public Int32 State { get => _State; set { if (OnPropertyChanging("State", value)) { _State = value; OnPropertyChanged("State"); } } }

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
            "Qlrq" => _Qlrq,
            "Qlsj" => _Qlsj,
            "Ksbm" => _Ksbm,
            "Yzgroupid" => _Yzgroupid,
            "Bhid" => _Bhid,
            "Yzbm" => _Yzbm,
            "DJ" => _DJ,
            "SL" => _SL,
            "Yfbm" => _Yfbm,
            "Fyrq" => _Fyrq,
            "State" => _State,
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
                case "Qlrq": _Qlrq = value.ToInt(); break;
                case "Qlsj": _Qlsj = value.ToInt(); break;
                case "Ksbm": _Ksbm = Convert.ToString(value); break;
                case "Yzgroupid": _Yzgroupid = value.ToInt(); break;
                case "Bhid": _Bhid = value.ToInt(); break;
                case "Yzbm": _Yzbm = Convert.ToString(value); break;
                case "DJ": _DJ = Convert.ToDecimal(value); break;
                case "SL": _SL = value.ToDouble(); break;
                case "Yfbm": _Yfbm = Convert.ToString(value); break;
                case "Fyrq": _Fyrq = value.ToInt(); break;
                case "State": _State = value.ToInt(); break;
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

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="bhid">病人ID</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<ZYYFQLD> Search(Int32 bhid, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (bhid >= 0) exp &= _.Bhid == bhid;
        exp &= _.UpdateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= SearchWhereByKeys(key);

        return FindAll(exp, page);
    }
    #endregion

    #region 字段名
    /// <summary>取得病人药房请领单分月表202001字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field ID = FindByName("ID");

        /// <summary>请领日期</summary>
        public static readonly Field Qlrq = FindByName("Qlrq");

        /// <summary>请领时间</summary>
        public static readonly Field Qlsj = FindByName("Qlsj");

        /// <summary>请领科室</summary>
        public static readonly Field Ksbm = FindByName("Ksbm");

        /// <summary>医嘱ID</summary>
        public static readonly Field Yzgroupid = FindByName("Yzgroupid");

        /// <summary>病人ID</summary>
        public static readonly Field Bhid = FindByName("Bhid");

        /// <summary>药品编码</summary>
        public static readonly Field Yzbm = FindByName("Yzbm");

        /// <summary>单价</summary>
        public static readonly Field DJ = FindByName("DJ");

        /// <summary>请领数量</summary>
        public static readonly Field SL = FindByName("SL");

        /// <summary>发药药房</summary>
        public static readonly Field Yfbm = FindByName("Yfbm");

        /// <summary>发药日期</summary>
        public static readonly Field Fyrq = FindByName("Fyrq");

        /// <summary>状态</summary>
        public static readonly Field State = FindByName("State");

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

    /// <summary>取得病人药房请领单分月表202001字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String ID = "ID";

        /// <summary>请领日期</summary>
        public const String Qlrq = "Qlrq";

        /// <summary>请领时间</summary>
        public const String Qlsj = "Qlsj";

        /// <summary>请领科室</summary>
        public const String Ksbm = "Ksbm";

        /// <summary>医嘱ID</summary>
        public const String Yzgroupid = "Yzgroupid";

        /// <summary>病人ID</summary>
        public const String Bhid = "Bhid";

        /// <summary>药品编码</summary>
        public const String Yzbm = "Yzbm";

        /// <summary>单价</summary>
        public const String DJ = "DJ";

        /// <summary>请领数量</summary>
        public const String SL = "SL";

        /// <summary>发药药房</summary>
        public const String Yfbm = "Yfbm";

        /// <summary>发药日期</summary>
        public const String Fyrq = "Fyrq";

        /// <summary>状态</summary>
        public const String State = "State";

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
