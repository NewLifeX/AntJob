using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace HisData
{
    /// <summary>病人药房请领单分月表202001</summary>
    [Serializable]
    [DataObject]
    [Description("病人药房请领单分月表202001")]
    [BindIndex("IX_ZYYFQLD_BHID", false, "BHID")]
    [BindTable("ZYYFQLD", Description = "病人药房请领单分月表202001", ConnName = "His", DbType = DatabaseType.None)]
    public partial class ZYYFQLD : IZYYFQLD
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private Int32 _Qlrq;
        /// <summary>请领日期</summary>
        [DisplayName("请领日期")]
        [Description("请领日期")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("QLRQ", "请领日期", "")]
        public Int32 Qlrq { get => _Qlrq; set { if (OnPropertyChanging(__.Qlrq, value)) { _Qlrq = value; OnPropertyChanged(__.Qlrq); } } }

        private Int32 _Qlsj;
        /// <summary>请领时间</summary>
        [DisplayName("请领时间")]
        [Description("请领时间")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("QLSJ", "请领时间", "")]
        public Int32 Qlsj { get => _Qlsj; set { if (OnPropertyChanging(__.Qlsj, value)) { _Qlsj = value; OnPropertyChanged(__.Qlsj); } } }

        private String _Ksbm;
        /// <summary>请领科室</summary>
        [DisplayName("请领科室")]
        [Description("请领科室")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("KSBM", "请领科室", "")]
        public String Ksbm { get => _Ksbm; set { if (OnPropertyChanging(__.Ksbm, value)) { _Ksbm = value; OnPropertyChanged(__.Ksbm); } } }

        private Int32 _Yzgroupid;
        /// <summary>医嘱ID</summary>
        [DisplayName("医嘱ID")]
        [Description("医嘱ID")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("YZGROUPID", "医嘱ID", "")]
        public Int32 Yzgroupid { get => _Yzgroupid; set { if (OnPropertyChanging(__.Yzgroupid, value)) { _Yzgroupid = value; OnPropertyChanged(__.Yzgroupid); } } }

        private Int32 _Bhid;
        /// <summary>病人ID</summary>
        [DisplayName("病人ID")]
        [Description("病人ID")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("BHID", "病人ID", "")]
        public Int32 Bhid { get => _Bhid; set { if (OnPropertyChanging(__.Bhid, value)) { _Bhid = value; OnPropertyChanged(__.Bhid); } } }

        private String _Yzbm;
        /// <summary>药品编码</summary>
        [DisplayName("药品编码")]
        [Description("药品编码")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("YZBM", "药品编码", "")]
        public String Yzbm { get => _Yzbm; set { if (OnPropertyChanging(__.Yzbm, value)) { _Yzbm = value; OnPropertyChanged(__.Yzbm); } } }

        private Decimal _DJ;
        /// <summary>单价</summary>
        [DisplayName("单价")]
        [Description("单价")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("DJ", "单价", "")]
        public Decimal DJ { get => _DJ; set { if (OnPropertyChanging(__.DJ, value)) { _DJ = value; OnPropertyChanged(__.DJ); } } }

        private Double _SL;
        /// <summary>请领数量</summary>
        [DisplayName("请领数量")]
        [Description("请领数量")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("SL", "请领数量", "")]
        public Double SL { get => _SL; set { if (OnPropertyChanging(__.SL, value)) { _SL = value; OnPropertyChanged(__.SL); } } }

        private String _Yfbm;
        /// <summary>发药药房</summary>
        [DisplayName("发药药房")]
        [Description("发药药房")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("YFBM", "发药药房", "")]
        public String Yfbm { get => _Yfbm; set { if (OnPropertyChanging(__.Yfbm, value)) { _Yfbm = value; OnPropertyChanged(__.Yfbm); } } }

        private Int32 _Fyrq;
        /// <summary>发药日期</summary>
        [DisplayName("发药日期")]
        [Description("发药日期")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("FYRQ", "发药日期", "")]
        public Int32 Fyrq { get => _Fyrq; set { if (OnPropertyChanging(__.Fyrq, value)) { _Fyrq = value; OnPropertyChanged(__.Fyrq); } } }

        private Int32 _State;
        /// <summary>状态</summary>
        [DisplayName("状态")]
        [Description("状态")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("STATE", "状态", "")]
        public Int32 State { get => _State; set { if (OnPropertyChanging(__.State, value)) { _State = value; OnPropertyChanged(__.State); } } }

        private String _Remark;
        /// <summary>内容</summary>
        [DisplayName("内容")]
        [Description("内容")]
        [DataObjectField(false, false, true, 500)]
        [BindColumn("Remark", "内容", "")]
        public String Remark { get => _Remark; set { if (OnPropertyChanging(__.Remark, value)) { _Remark = value; OnPropertyChanged(__.Remark); } } }

        private String _CreateUser;
        /// <summary>创建者</summary>
        [DisplayName("创建者")]
        [Description("创建者")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("CreateUser", "创建者", "")]
        public String CreateUser { get => _CreateUser; set { if (OnPropertyChanging(__.CreateUser, value)) { _CreateUser = value; OnPropertyChanged(__.CreateUser); } } }

        private Int32 _CreateUserID;
        /// <summary>创建者</summary>
        [DisplayName("创建者")]
        [Description("创建者")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("CreateUserID", "创建者", "")]
        public Int32 CreateUserID { get => _CreateUserID; set { if (OnPropertyChanging(__.CreateUserID, value)) { _CreateUserID = value; OnPropertyChanged(__.CreateUserID); } } }

        private DateTime _CreateTime;
        /// <summary>创建时间</summary>
        [DisplayName("创建时间")]
        [Description("创建时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("CreateTime", "创建时间", "")]
        public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging(__.CreateTime, value)) { _CreateTime = value; OnPropertyChanged(__.CreateTime); } } }

        private String _CreateIP;
        /// <summary>创建地址</summary>
        [DisplayName("创建地址")]
        [Description("创建地址")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("CreateIP", "创建地址", "")]
        public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging(__.CreateIP, value)) { _CreateIP = value; OnPropertyChanged(__.CreateIP); } } }

        private String _UpdateUser;
        /// <summary>更新者</summary>
        [DisplayName("更新者")]
        [Description("更新者")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("UpdateUser", "更新者", "")]
        public String UpdateUser { get => _UpdateUser; set { if (OnPropertyChanging(__.UpdateUser, value)) { _UpdateUser = value; OnPropertyChanged(__.UpdateUser); } } }

        private Int32 _UpdateUserID;
        /// <summary>更新者</summary>
        [DisplayName("更新者")]
        [Description("更新者")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("UpdateUserID", "更新者", "")]
        public Int32 UpdateUserID { get => _UpdateUserID; set { if (OnPropertyChanging(__.UpdateUserID, value)) { _UpdateUserID = value; OnPropertyChanged(__.UpdateUserID); } } }

        private DateTime _UpdateTime;
        /// <summary>更新时间</summary>
        [DisplayName("更新时间")]
        [Description("更新时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("UpdateTime", "更新时间", "")]
        public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging(__.UpdateTime, value)) { _UpdateTime = value; OnPropertyChanged(__.UpdateTime); } } }

        private String _UpdateIP;
        /// <summary>更新地址</summary>
        [DisplayName("更新地址")]
        [Description("更新地址")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("UpdateIP", "更新地址", "")]
        public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging(__.UpdateIP, value)) { _UpdateIP = value; OnPropertyChanged(__.UpdateIP); } } }
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
                    case __.ID: return _ID;
                    case __.Qlrq: return _Qlrq;
                    case __.Qlsj: return _Qlsj;
                    case __.Ksbm: return _Ksbm;
                    case __.Yzgroupid: return _Yzgroupid;
                    case __.Bhid: return _Bhid;
                    case __.Yzbm: return _Yzbm;
                    case __.DJ: return _DJ;
                    case __.SL: return _SL;
                    case __.Yfbm: return _Yfbm;
                    case __.Fyrq: return _Fyrq;
                    case __.State: return _State;
                    case __.Remark: return _Remark;
                    case __.CreateUser: return _CreateUser;
                    case __.CreateUserID: return _CreateUserID;
                    case __.CreateTime: return _CreateTime;
                    case __.CreateIP: return _CreateIP;
                    case __.UpdateUser: return _UpdateUser;
                    case __.UpdateUserID: return _UpdateUserID;
                    case __.UpdateTime: return _UpdateTime;
                    case __.UpdateIP: return _UpdateIP;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID: _ID = value.ToInt(); break;
                    case __.Qlrq: _Qlrq = value.ToInt(); break;
                    case __.Qlsj: _Qlsj = value.ToInt(); break;
                    case __.Ksbm: _Ksbm = Convert.ToString(value); break;
                    case __.Yzgroupid: _Yzgroupid = value.ToInt(); break;
                    case __.Bhid: _Bhid = value.ToInt(); break;
                    case __.Yzbm: _Yzbm = Convert.ToString(value); break;
                    case __.DJ: _DJ = Convert.ToDecimal(value); break;
                    case __.SL: _SL = value.ToDouble(); break;
                    case __.Yfbm: _Yfbm = Convert.ToString(value); break;
                    case __.Fyrq: _Fyrq = value.ToInt(); break;
                    case __.State: _State = value.ToInt(); break;
                    case __.Remark: _Remark = Convert.ToString(value); break;
                    case __.CreateUser: _CreateUser = Convert.ToString(value); break;
                    case __.CreateUserID: _CreateUserID = value.ToInt(); break;
                    case __.CreateTime: _CreateTime = value.ToDateTime(); break;
                    case __.CreateIP: _CreateIP = Convert.ToString(value); break;
                    case __.UpdateUser: _UpdateUser = Convert.ToString(value); break;
                    case __.UpdateUserID: _UpdateUserID = value.ToInt(); break;
                    case __.UpdateTime: _UpdateTime = value.ToDateTime(); break;
                    case __.UpdateIP: _UpdateIP = Convert.ToString(value); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得病人药房请领单分月表202001字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>请领日期</summary>
            public static readonly Field Qlrq = FindByName(__.Qlrq);

            /// <summary>请领时间</summary>
            public static readonly Field Qlsj = FindByName(__.Qlsj);

            /// <summary>请领科室</summary>
            public static readonly Field Ksbm = FindByName(__.Ksbm);

            /// <summary>医嘱ID</summary>
            public static readonly Field Yzgroupid = FindByName(__.Yzgroupid);

            /// <summary>病人ID</summary>
            public static readonly Field Bhid = FindByName(__.Bhid);

            /// <summary>药品编码</summary>
            public static readonly Field Yzbm = FindByName(__.Yzbm);

            /// <summary>单价</summary>
            public static readonly Field DJ = FindByName(__.DJ);

            /// <summary>请领数量</summary>
            public static readonly Field SL = FindByName(__.SL);

            /// <summary>发药药房</summary>
            public static readonly Field Yfbm = FindByName(__.Yfbm);

            /// <summary>发药日期</summary>
            public static readonly Field Fyrq = FindByName(__.Fyrq);

            /// <summary>状态</summary>
            public static readonly Field State = FindByName(__.State);

            /// <summary>内容</summary>
            public static readonly Field Remark = FindByName(__.Remark);

            /// <summary>创建者</summary>
            public static readonly Field CreateUser = FindByName(__.CreateUser);

            /// <summary>创建者</summary>
            public static readonly Field CreateUserID = FindByName(__.CreateUserID);

            /// <summary>创建时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>创建地址</summary>
            public static readonly Field CreateIP = FindByName(__.CreateIP);

            /// <summary>更新者</summary>
            public static readonly Field UpdateUser = FindByName(__.UpdateUser);

            /// <summary>更新者</summary>
            public static readonly Field UpdateUserID = FindByName(__.UpdateUserID);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            /// <summary>更新地址</summary>
            public static readonly Field UpdateIP = FindByName(__.UpdateIP);

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

    /// <summary>病人药房请领单分月表202001接口</summary>
    public partial interface IZYYFQLD
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>请领日期</summary>
        Int32 Qlrq { get; set; }

        /// <summary>请领时间</summary>
        Int32 Qlsj { get; set; }

        /// <summary>请领科室</summary>
        String Ksbm { get; set; }

        /// <summary>医嘱ID</summary>
        Int32 Yzgroupid { get; set; }

        /// <summary>病人ID</summary>
        Int32 Bhid { get; set; }

        /// <summary>药品编码</summary>
        String Yzbm { get; set; }

        /// <summary>单价</summary>
        Decimal DJ { get; set; }

        /// <summary>请领数量</summary>
        Double SL { get; set; }

        /// <summary>发药药房</summary>
        String Yfbm { get; set; }

        /// <summary>发药日期</summary>
        Int32 Fyrq { get; set; }

        /// <summary>状态</summary>
        Int32 State { get; set; }

        /// <summary>内容</summary>
        String Remark { get; set; }

        /// <summary>创建者</summary>
        String CreateUser { get; set; }

        /// <summary>创建者</summary>
        Int32 CreateUserID { get; set; }

        /// <summary>创建时间</summary>
        DateTime CreateTime { get; set; }

        /// <summary>创建地址</summary>
        String CreateIP { get; set; }

        /// <summary>更新者</summary>
        String UpdateUser { get; set; }

        /// <summary>更新者</summary>
        Int32 UpdateUserID { get; set; }

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