using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using NewLife.Web;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;
using XCode.Membership;

namespace HisData
{
    /// <summary>病人医嘱明细信息</summary>
    public partial class ZYBHYZ1 : Entity<ZYBHYZ1>
    {
        #region 对象操作
        static ZYBHYZ1()
        {
            // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
            //var df = Meta.Factory.AdditionalFields;
            //df.Add(__.Dgroupid);

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
            // 货币保留6位小数
            DJ = Math.Round(DJ, 6);
            SL = Math.Round(SL, 6);
            FY = Math.Round(FY, 6);
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
            // CheckExist(isNew, __.Dgroupid, __.Yzbm);
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    if (Meta.Session.Count > 0) return;

        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化ZYBHYZ1[病人医嘱明细信息]数据……");

        //    var entity = new ZYBHYZ1();
        //    entity.ID = 0;
        //    entity.Dgroupid = 0;
        //    entity.Yzbm = "abc";
        //    entity.Yzmc = "abc";
        //    entity.DJ = 0.0;
        //    entity.SL = 0.0;
        //    entity.FY = 0.0;
        //    entity.State = 0;
        //    entity.CreateUser = "abc";
        //    entity.CreateUserID = 0;
        //    entity.CreateTime = DateTime.Now;
        //    entity.CreateIP = "abc";
        //    entity.UpdateUser = "abc";
        //    entity.UpdateUserID = 0;
        //    entity.UpdateTime = DateTime.Now;
        //    entity.UpdateIP = "abc";
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化ZYBHYZ1[病人医嘱明细信息]数据！");
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
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static ZYBHYZ1 FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据医嘱组号、医嘱编码查找</summary>
        /// <param name="dgroupid">医嘱组号</param>
        /// <param name="yzbm">医嘱编码</param>
        /// <returns>实体对象</returns>
        public static ZYBHYZ1 FindByDgroupidAndYzbm(Int32 dgroupid, String yzbm)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Dgroupid == dgroupid && e.Yzbm == yzbm);

            return Find(_.Dgroupid == dgroupid & _.Yzbm == yzbm);
        }

        /// <summary>根据医嘱组号查找</summary>
        /// <param name="dgroupid">医嘱组号</param>
        /// <returns>实体列表</returns>
        public static IList<ZYBHYZ1> FindAllByDgroupid(Int32 dgroupid)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Dgroupid == dgroupid);

            return FindAll(_.Dgroupid == dgroupid);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="dgroupid">医嘱组号</param>
        /// <param name="yzbm">医嘱编码</param>
        /// <param name="key">关键字</param>
        /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
        /// <returns>实体列表</returns>
        public static IList<ZYBHYZ1> Search(Int32 dgroupid, String yzbm, String key, PageParameter page)
        {
            var exp = new WhereExpression();

            if (dgroupid >= 0) exp &= _.Dgroupid == dgroupid;
            if (!yzbm.IsNullOrEmpty()) exp &= _.Yzbm == yzbm;
            if (!key.IsNullOrEmpty()) exp &= _.Yzmc.Contains(key) | _.CreateUser.Contains(key) | _.CreateIP.Contains(key) | _.UpdateUser.Contains(key) | _.UpdateIP.Contains(key);

            return FindAll(exp, page);
        }

        // Select Count(ID) as ID,Category From ZYBHYZ1 Where CreateTime>'2020-01-24 00:00:00' Group By Category Order By ID Desc limit 20
        //static readonly FieldCache<ZYBHYZ1> _CategoryCache = new FieldCache<ZYBHYZ1>(_.Category)
        //{
        //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        //};

        ///// <summary>获取类别列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        ///// <returns></returns>
        //public static IDictionary<String, String> GetCategoryList() => _CategoryCache.FindAllName();
        #endregion

        #region 业务操作
        #endregion
    }
}