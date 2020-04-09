using System;
using System.Collections.Generic;
using System.ComponentModel;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace HisData
{
    /// <summary>病人医嘱信息</summary>
    [Serializable]
    [DataObject]
    [Description("病人医嘱信息")]
    [BindIndex("IU_ZYBHYZ0_BHID_MGROUPID", true, "BHID,MGROUPID")]
    [BindIndex("IX_ZYBHYZ0_BHID", false, "BHID")]
    [BindTable("ZYBHYZ0", Description = "病人医嘱信息", ConnName = "His", DbType = DatabaseType.None)]
    public partial class ZYBHYZ0 : IZYBHYZ0
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private Int32 _Bhid;
        /// <summary>病人ID</summary>
        [DisplayName("病人ID")]
        [Description("病人ID")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("BHID", "病人ID", "")]
        public Int32 Bhid { get => _Bhid; set { if (OnPropertyChanging(__.Bhid, value)) { _Bhid = value; OnPropertyChanged(__.Bhid); } } }

        private Int32 _Mgroupid;
        /// <summary>医嘱组号</summary>
        [DisplayName("医嘱组号")]
        [Description("医嘱组号")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("MGROUPID", "医嘱组号", "", Master = true)]
        public Int32 Mgroupid { get => _Mgroupid; set { if (OnPropertyChanging(__.Mgroupid, value)) { _Mgroupid = value; OnPropertyChanged(__.Mgroupid); } } }

        private DateTime _Kyzrq;
        /// <summary>开医嘱日期</summary>
        [DisplayName("开医嘱日期")]
        [Description("开医嘱日期")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("KYZRQ", "开医嘱日期", "")]
        public DateTime Kyzrq { get => _Kyzrq; set { if (OnPropertyChanging(__.Kyzrq, value)) { _Kyzrq = value; OnPropertyChanged(__.Kyzrq); } } }

        private DateTime _Tyzrq;
        /// <summary>停医嘱日期</summary>
        [DisplayName("停医嘱日期")]
        [Description("停医嘱日期")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("TYZRQ", "停医嘱日期", "")]
        public DateTime Tyzrq { get => _Tyzrq; set { if (OnPropertyChanging(__.Tyzrq, value)) { _Tyzrq = value; OnPropertyChanged(__.Tyzrq); } } }

        private String _Kyzys;
        /// <summary>开医嘱医生</summary>
        [DisplayName("开医嘱医生")]
        [Description("开医嘱医生")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("KYZYS", "开医嘱医生", "")]
        public String Kyzys { get => _Kyzys; set { if (OnPropertyChanging(__.Kyzys, value)) { _Kyzys = value; OnPropertyChanged(__.Kyzys); } } }

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
                    case __.Bhid: return _Bhid;
                    case __.Mgroupid: return _Mgroupid;
                    case __.Kyzrq: return _Kyzrq;
                    case __.Tyzrq: return _Tyzrq;
                    case __.Kyzys: return _Kyzys;
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
                    case __.Bhid: _Bhid = value.ToInt(); break;
                    case __.Mgroupid: _Mgroupid = value.ToInt(); break;
                    case __.Kyzrq: _Kyzrq = value.ToDateTime(); break;
                    case __.Tyzrq: _Tyzrq = value.ToDateTime(); break;
                    case __.Kyzys: _Kyzys = Convert.ToString(value); break;
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
        /// <summary>取得病人医嘱信息字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>病人ID</summary>
            public static readonly Field Bhid = FindByName(__.Bhid);

            /// <summary>医嘱组号</summary>
            public static readonly Field Mgroupid = FindByName(__.Mgroupid);

            /// <summary>开医嘱日期</summary>
            public static readonly Field Kyzrq = FindByName(__.Kyzrq);

            /// <summary>停医嘱日期</summary>
            public static readonly Field Tyzrq = FindByName(__.Tyzrq);

            /// <summary>开医嘱医生</summary>
            public static readonly Field Kyzys = FindByName(__.Kyzys);

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

        /// <summary>取得病人医嘱信息字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>病人ID</summary>
            public const String Bhid = "Bhid";

            /// <summary>医嘱组号</summary>
            public const String Mgroupid = "Mgroupid";

            /// <summary>开医嘱日期</summary>
            public const String Kyzrq = "Kyzrq";

            /// <summary>停医嘱日期</summary>
            public const String Tyzrq = "Tyzrq";

            /// <summary>开医嘱医生</summary>
            public const String Kyzys = "Kyzys";

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

    /// <summary>病人医嘱信息接口</summary>
    public partial interface IZYBHYZ0
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>病人ID</summary>
        Int32 Bhid { get; set; }

        /// <summary>医嘱组号</summary>
        Int32 Mgroupid { get; set; }

        /// <summary>开医嘱日期</summary>
        DateTime Kyzrq { get; set; }

        /// <summary>停医嘱日期</summary>
        DateTime Tyzrq { get; set; }

        /// <summary>开医嘱医生</summary>
        String Kyzys { get; set; }

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