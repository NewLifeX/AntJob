using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Membership;

namespace AntJob.Data.Entity
{
    /// <summary>应用配置。各应用的配置数据</summary>
    public partial class AppConfig : EntityBase<AppConfig>
    {
        #region 对象操作
        static AppConfig()
        {
            // 累加字段
            //var df = Meta.Factory.AdditionalFields;
            //df.Add(__.AppID);

            // 过滤器 UserModule、TimeModule、IPModule
            Meta.Modules.Add<UserModule>();
            Meta.Modules.Add<TimeModule>();
            Meta.Modules.Add<IPModule>();
        }

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;

            // 在新插入数据或者修改了指定字段时进行修正
            // 处理当前已登录用户信息，可以由UserModule过滤器代劳
            /*var user = ManageProvider.User;
            if (user != null)
            {
                if (isNew && !Dirtys[nameof(CreateUserID)]) CreateUserID = user.ID;
                if (!Dirtys[nameof(UpdateUserID)]) UpdateUserID = user.ID;
            }*/
            //if (isNew && !Dirtys[nameof(CreateTime)]) CreateTime = DateTime.Now;
            //if (!Dirtys[nameof(UpdateTime)]) UpdateTime = DateTime.Now;
            //if (isNew && !Dirtys[nameof(CreateIP)]) CreateIP = ManageProvider.UserHost;
            //if (!Dirtys[nameof(UpdateIP)]) UpdateIP = ManageProvider.UserHost;

            // 检查唯一索引
            // CheckExist(isNew, __.AppID, __.Name);
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    if (Meta.Session.Count > 0) return;

        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化AppConfig[应用配置]数据……");

        //    var entity = new AppConfig();
        //    entity.ID = 0;
        //    entity.AppID = 0;
        //    entity.Name = "abc";
        //    entity.Content = "abc";
        //    entity.CreateUserID = 0;
        //    entity.CreateTime = DateTime.Now;
        //    entity.CreateIP = "abc";
        //    entity.UpdateUserID = 0;
        //    entity.UpdateTime = DateTime.Now;
        //    entity.UpdateIP = "abc";
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化AppConfig[应用配置]数据！");
        //}

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnDelete()
        //{
        //    return base.OnDelete();
        //}
        #endregion

        #region 扩展属性
        /// <summary>设备编号</summary>
        [XmlIgnore, IgnoreDataMember]
        //[ScriptIgnore]
        public App App => Extends.Get(nameof(App), k => App.FindByID(AppID));

        /// <summary>设备编号</summary>
        [XmlIgnore, IgnoreDataMember]
        //[ScriptIgnore]
        [DisplayName("设备编号")]
        [Map(__.AppID, typeof(App), "ID")]
        public String AppName => App?.Name;
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static AppConfig FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据设备编号、名称查找</summary>
        /// <param name="appid">设备编号</param>
        /// <param name="name">名称</param>
        /// <returns>实体对象</returns>
        public static AppConfig FindByAppIDAndName(Int32 appid, String name)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.AppID == appid && e.Name == name);

            return Find(_.AppID == appid & _.Name == name);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="appid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="key"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IEnumerable<AppConfig> Search(Int32 appid, DateTime start, DateTime end, String key, PageParameter p)
        {
            var exp = new WhereExpression();

            if (appid > 0) exp &= _.AppID == appid.ToInt();
            if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key);
            exp &= _.CreateTime.Between(start, end);

            return FindAll(exp, p);
        }
        #endregion

        #region 业务操作
        /// <summary>类别名实体缓存，异步，缓存10分钟</summary>
        static readonly FieldCache<AppConfig> NameCache = new FieldCache<AppConfig>(_.Name);

        /// <summary>获取所有类别名称</summary>
        /// <returns></returns>
        public static IDictionary<String, String> FindAllName() => NameCache.FindAllName();
        #endregion
    }
}