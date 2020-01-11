using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Membership;

namespace AntJob.Data.Entity
{
    /// <summary>应用历史</summary>
    public partial class AppHistory : EntityBase<AppHistory>
    {
        #region 对象操作
        static AppHistory()
        {
            // 累加字段
            //var df = Meta.Factory.AdditionalFields;
            //df.Add(__.AppID);

            // 过滤器 UserModule、TimeModule、IPModule
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
            //if (isNew && !Dirtys[nameof(CreateTime)]) CreateTime = DateTime.Now;
            //if (isNew && !Dirtys[nameof(CreateIP)]) CreateIP = ManageProvider.UserHost;
        }
        #endregion

        #region 扩展属性
        /// <summary>应用</summary>
        [XmlIgnore, IgnoreDataMember]
        //[ScriptIgnore]
        public App App => Extends.Get(nameof(App), k => App.FindByID(AppID));

        /// <summary>应用</summary>
        [XmlIgnore, IgnoreDataMember]
        //[ScriptIgnore]
        [DisplayName("应用")]
        [Map(__.AppID, typeof(App), "ID")]
        public String AppName => App?.Name;
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static AppHistory FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据应用、操作查找</summary>
        /// <param name="appid">应用</param>
        /// <param name="action">操作</param>
        /// <returns>实体列表</returns>
        public static IList<AppHistory> FindAllByAppIDAndAction(Int32 appid, String action)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appid && e.Action == action);

            return FindAll(_.AppID == appid & _.Action == action);
        }
        #endregion

        #region 高级查询
        #endregion

        #region 高级查询
        /// <summary>高级搜索</summary>
        /// <param name="appid"></param>
        /// <param name="action"></param>
        /// <param name="success"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="key"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static IList<AppHistory> Search(Int32 appid, String action, Boolean? success, DateTime start, DateTime end, String key, PageParameter page)
        {
            var exp = new WhereExpression();

            if (appid >= 0) exp &= _.AppID == appid;
            if (!action.IsNullOrEmpty()) exp &= _.Action == action;
            if (success != null) exp &= _.Success == success;

            exp &= _.CreateTime.Between(start, end);

            if (!key.IsNullOrEmpty())
            {
                exp &= (_.Name.Contains(key) | _.Remark.Contains(key) | _.CreateIP.Contains(key));
            }

            return FindAll(exp, page);
        }
        #endregion

        #region 扩展操作
        /// <summary>类别名实体缓存，异步，缓存10分钟</summary>
        static readonly FieldCache<AppHistory> ActionCache = new FieldCache<AppHistory>(_.Action);

        /// <summary>获取所有类别名称</summary>
        /// <returns></returns>
        public static IDictionary<String, String> FindAllAction() => ActionCache.FindAllName();
        #endregion

        #region 业务
        /// <summary>创建日志</summary>
        /// <param name="app"></param>
        /// <param name="action"></param>
        /// <param name="success"></param>
        /// <param name="remark"></param>
        /// <param name="creator"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static AppHistory Create(IApp app, String action, Boolean success, String remark, String creator, String ip)
        {
            if (app == null) app = new App();

            var hi = new AppHistory
            {
                AppID = app.ID,
                Name = app.Name,
                Action = action,
                Success = success,
                Server = creator,

                Version = app.Version,
                CompileTime = app.CompileTime,

                Remark = remark,

                CreateTime = DateTime.Now,
                CreateIP = ip,
            };

            hi.SaveAsync();

            return hi;
        }
        #endregion 
    }
}