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
        static readonly FieldCache<AppConfig> NameCache = new FieldCache<AppConfig>(__.Name);

        /// <summary>获取所有类别名称</summary>
        /// <returns></returns>
        public static IDictionary<String, String> FindAllName() => NameCache.FindAllName();
        #endregion
    }
}