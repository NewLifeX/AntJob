using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace HisData
{
    /// <summary>病人医嘱明细信息</summary>
    [Serializable]
    [DataObject]
    [Description("病人医嘱明细信息")]
    [BindIndex("IU_ZYBHYZ1_DGROUPID_YZBM", true, "DGROUPID,YZBM")]
    [BindIndex("IX_ZYBHYZ1_DGROUPID", false, "DGROUPID")]
    [BindTable("ZYBHYZ1", Description = "病人医嘱明细信息", ConnName = "His", DbType = DatabaseType.None)]
    public partial class ZYBHYZ1 : IZYBHYZ1
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private Int32 _Dgroupid;
        /// <summary>医嘱组号</summary>
        [DisplayName("医嘱组号")]
        [Description("医嘱组号")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("DGROUPID", "医嘱组号", "", Master = true)]
        public Int32 Dgroupid { get => _Dgroupid; set { if (OnPropertyChanging(__.Dgroupid, value)) { _Dgroupid = value; OnPropertyChanged(__.Dgroupid); } } }

        private String _Yzbm;
        /// <summary>医嘱编码</summary>
        [DisplayName("医嘱编码")]
        [Description("医嘱编码")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("YZBM", "医嘱编码", "")]
        public String Yzbm { get => _Yzbm; set { if (OnPropertyChanging(__.Yzbm, value)) { _Yzbm = value; OnPropertyChanged(__.Yzbm); } } }

        private String _Yzmc;
        /// <summary>医嘱名称</summary>
        [DisplayName("医嘱名称")]
        [Description("医嘱名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("YZMC", "医嘱名称", "")]
        public String Yzmc { get => _Yzmc; set { if (OnPropertyChanging(__.Yzmc, value)) { _Yzmc = value; OnPropertyChanged(__.Yzmc); } } }

        private Decimal _DJ;
        /// <summary>单价</summary>
        [DisplayName("单价")]
        [Description("单价")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("DJ", "单价", "")]
        public Decimal DJ { get => _DJ; set { if (OnPropertyChanging(__.DJ, value)) { _DJ = value; OnPropertyChanged(__.DJ); } } }

        private Double _SL;
        /// <summary>数量</summary>
        [DisplayName("数量")]
        [Description("数量")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("SL", "数量", "")]
        public Double SL { get => _SL; set { if (OnPropertyChanging(__.SL, value)) { _SL = value; OnPropertyChanged(__.SL); } } }

        private Decimal _FY;
        /// <summary>费用</summary>
        [DisplayName("费用")]
        [Description("费用")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("FY", "费用", "")]
        public Decimal FY { get => _FY; set { if (OnPropertyChanging(__.FY, value)) { _FY = value; OnPropertyChanged(__.FY); } } }

        private Int32 _State;
        /// <summary>状态</summary>
        [DisplayName("状态")]
        [Description("状态")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("STATE", "状态", "")]
        public Int32 State { get => _State; set { if (OnPropertyChanging(__.State, value)) { _State = value; OnPropertyChanged(__.State); } } }

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
                    case __.Dgroupid: return _Dgroupid;
                    case __.Yzbm: return _Yzbm;
                    case __.Yzmc: return _Yzmc;
                    case __.DJ: return _DJ;
                    case __.SL: return _SL;
                    case __.FY: return _FY;
                    case __.State: return _State;
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
                    case __.Dgroupid: _Dgroupid = value.ToInt(); break;
                    case __.Yzbm: _Yzbm = Convert.ToString(value); break;
                    case __.Yzmc: _Yzmc = Convert.ToString(value); break;
                    case __.DJ: _DJ = Convert.ToDecimal(value); break;
                    case __.SL: _SL = value.ToDouble(); break;
                    case __.FY: _FY = Convert.ToDecimal(value); break;
                    case __.State: _State = value.ToInt(); break;
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
        /// <summary>取得病人医嘱明细信息字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>医嘱组号</summary>
            public static readonly Field Dgroupid = FindByName(__.Dgroupid);

            /// <summary>医嘱编码</summary>
            public static readonly Field Yzbm = FindByName(__.Yzbm);

            /// <summary>医嘱名称</summary>
            public static readonly Field Yzmc = FindByName(__.Yzmc);

            /// <summary>单价</summary>
            public static readonly Field DJ = FindByName(__.DJ);

            /// <summary>数量</summary>
            public static readonly Field SL = FindByName(__.SL);

            /// <summary>费用</summary>
            public static readonly Field FY = FindByName(__.FY);

            /// <summary>状态</summary>
            public static readonly Field State = FindByName(__.State);

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

    /// <summary>病人医嘱明细信息接口</summary>
    public partial interface IZYBHYZ1
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>医嘱组号</summary>
        Int32 Dgroupid { get; set; }

        /// <summary>医嘱编码</summary>
        String Yzbm { get; set; }

        /// <summary>医嘱名称</summary>
        String Yzmc { get; set; }

        /// <summary>单价</summary>
        Decimal DJ { get; set; }

        /// <summary>数量</summary>
        Double SL { get; set; }

        /// <summary>费用</summary>
        Decimal FY { get; set; }

        /// <summary>状态</summary>
        Int32 State { get; set; }

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