using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace HisData
{
    /// <summary>收费字典</summary>
    [Serializable]
    [DataObject]
    [Description("收费字典")]
    [BindIndex("IU_ZDSF_BM", true, "BM")]
    [BindTable("ZDSF", Description = "收费字典", ConnName = "His", DbType = DatabaseType.None)]
    public partial class ZDSF : IZDSF
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private String _BM;
        /// <summary>编码</summary>
        [DisplayName("编码")]
        [Description("编码")]
        [DataObjectField(false, false, false, 50)]
        [BindColumn("BM", "编码", "", Master = true)]
        public String BM { get => _BM; set { if (OnPropertyChanging(__.BM, value)) { _BM = value; OnPropertyChanged(__.BM); } } }

        private String _DH;
        /// <summary>拼音码</summary>
        [DisplayName("拼音码")]
        [Description("拼音码")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("DH", "拼音码", "")]
        public String DH { get => _DH; set { if (OnPropertyChanging(__.DH, value)) { _DH = value; OnPropertyChanged(__.DH); } } }

        private String _MC;
        /// <summary>名称</summary>
        [DisplayName("名称")]
        [Description("名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("MC", "名称", "")]
        public String MC { get => _MC; set { if (OnPropertyChanging(__.MC, value)) { _MC = value; OnPropertyChanged(__.MC); } } }

        private Decimal _DJ;
        /// <summary>单价</summary>
        [DisplayName("单价")]
        [Description("单价")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("DJ", "单价", "")]
        public Decimal DJ { get => _DJ; set { if (OnPropertyChanging(__.DJ, value)) { _DJ = value; OnPropertyChanged(__.DJ); } } }

        private String _DW;
        /// <summary>单位</summary>
        [DisplayName("单位")]
        [Description("单位")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("DW", "单位", "")]
        public String DW { get => _DW; set { if (OnPropertyChanging(__.DW, value)) { _DW = value; OnPropertyChanged(__.DW); } } }

        private Int32 _Mzyflb;
        /// <summary>门诊费用类别</summary>
        [DisplayName("门诊费用类别")]
        [Description("门诊费用类别")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MZYFLB", "门诊费用类别", "")]
        public Int32 Mzyflb { get => _Mzyflb; set { if (OnPropertyChanging(__.Mzyflb, value)) { _Mzyflb = value; OnPropertyChanged(__.Mzyflb); } } }

        private Int32 _Zyfylb;
        /// <summary>住院费用类别</summary>
        [DisplayName("住院费用类别")]
        [Description("住院费用类别")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ZYFYLB", "住院费用类别", "")]
        public Int32 Zyfylb { get => _Zyfylb; set { if (OnPropertyChanging(__.Zyfylb, value)) { _Zyfylb = value; OnPropertyChanged(__.Zyfylb); } } }

        private Double _Zfbl;
        /// <summary>自费比例</summary>
        [DisplayName("自费比例")]
        [Description("自费比例")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ZFBL", "自费比例", "")]
        public Double Zfbl { get => _Zfbl; set { if (OnPropertyChanging(__.Zfbl, value)) { _Zfbl = value; OnPropertyChanged(__.Zfbl); } } }

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
                    case __.BM: return _BM;
                    case __.DH: return _DH;
                    case __.MC: return _MC;
                    case __.DJ: return _DJ;
                    case __.DW: return _DW;
                    case __.Mzyflb: return _Mzyflb;
                    case __.Zyfylb: return _Zyfylb;
                    case __.Zfbl: return _Zfbl;
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
                    case __.BM: _BM = Convert.ToString(value); break;
                    case __.DH: _DH = Convert.ToString(value); break;
                    case __.MC: _MC = Convert.ToString(value); break;
                    case __.DJ: _DJ = Convert.ToDecimal(value); break;
                    case __.DW: _DW = Convert.ToString(value); break;
                    case __.Mzyflb: _Mzyflb = value.ToInt(); break;
                    case __.Zyfylb: _Zyfylb = value.ToInt(); break;
                    case __.Zfbl: _Zfbl = value.ToDouble(); break;
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
        /// <summary>取得收费字典字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>编码</summary>
            public static readonly Field BM = FindByName(__.BM);

            /// <summary>拼音码</summary>
            public static readonly Field DH = FindByName(__.DH);

            /// <summary>名称</summary>
            public static readonly Field MC = FindByName(__.MC);

            /// <summary>单价</summary>
            public static readonly Field DJ = FindByName(__.DJ);

            /// <summary>单位</summary>
            public static readonly Field DW = FindByName(__.DW);

            /// <summary>门诊费用类别</summary>
            public static readonly Field Mzyflb = FindByName(__.Mzyflb);

            /// <summary>住院费用类别</summary>
            public static readonly Field Zyfylb = FindByName(__.Zyfylb);

            /// <summary>自费比例</summary>
            public static readonly Field Zfbl = FindByName(__.Zfbl);

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

        /// <summary>取得收费字典字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>编码</summary>
            public const String BM = "BM";

            /// <summary>拼音码</summary>
            public const String DH = "DH";

            /// <summary>名称</summary>
            public const String MC = "MC";

            /// <summary>单价</summary>
            public const String DJ = "DJ";

            /// <summary>单位</summary>
            public const String DW = "DW";

            /// <summary>门诊费用类别</summary>
            public const String Mzyflb = "Mzyflb";

            /// <summary>住院费用类别</summary>
            public const String Zyfylb = "Zyfylb";

            /// <summary>自费比例</summary>
            public const String Zfbl = "Zfbl";

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

    /// <summary>收费字典接口</summary>
    public partial interface IZDSF
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>编码</summary>
        String BM { get; set; }

        /// <summary>拼音码</summary>
        String DH { get; set; }

        /// <summary>名称</summary>
        String MC { get; set; }

        /// <summary>单价</summary>
        Decimal DJ { get; set; }

        /// <summary>单位</summary>
        String DW { get; set; }

        /// <summary>门诊费用类别</summary>
        Int32 Mzyflb { get; set; }

        /// <summary>住院费用类别</summary>
        Int32 Zyfylb { get; set; }

        /// <summary>自费比例</summary>
        Double Zfbl { get; set; }

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