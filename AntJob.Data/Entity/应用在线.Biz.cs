using System;
using System.Collections.Generic;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace AntJob.Data.Entity;

/// <summary>应用在线。各应用多实例在线</summary>
public partial class AppOnline : EntityBase<AppOnline>
{
    #region 对象操作
    static AppOnline()
    {
        // 累加字段
        //var df = Meta.Factory.AdditionalFields;
        //df.Add(__.AppID);

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TraceModule>();
    }
    #endregion

    #region 扩展属性
    ///// <summary>应用</summary>
    //[XmlIgnore]
    ////[ScriptIgnore]
    //public App App => Extends.Get(nameof(App), k => App.FindByID(AppID));

    ///// <summary>应用</summary>
    //[XmlIgnore]
    ////[ScriptIgnore]
    //[DisplayName("应用")]
    //[Map(__.AppID)]
    //public String AppName => App?.Name;
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static AppOnline FindByID(Int32 id)
    {
        if (id <= 0) return null;

        return Find(_.ID == id);
    }

    /// <summary>根据实例查找</summary>
    /// <param name="instance">实例</param>
    /// <returns>实体对象</returns>
    public static AppOnline FindByInstance(String instance)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Instance == instance);

        return Find(_.Instance == instance);
    }

    /// <summary>根据应用查找</summary>
    /// <param name="appid">应用</param>
    /// <returns>实体列表</returns>
    public static IList<AppOnline> FindAllByAppID(Int32 appid)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appid);

        return FindAll(_.AppID == appid);
    }

    /// <summary>根据客户端查找</summary>
    /// <param name="client">客户端</param>
    /// <returns>实体列表</returns>
    public static IList<AppOnline> FindAllByClient(String client)
    {
        if (client.IsNullOrEmpty()) return new List<AppOnline>();

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Client.EqualIgnoreCase(client));

        return FindAll(_.Client == client);
    }
    #endregion

    #region 高级查询
    /// <summary>
    /// 高级查询
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<AppOnline> Search(Int32 appid, DateTime start, DateTime end, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (appid > 0) exp &= _.AppID == appid.ToInt();
        if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key) | _.Instance.Contains(key);
        exp &= _.CreateTime.Between(start, end);

        return FindAll(exp, p);
    }

    /// <summary>
    /// 获取UpdateTime距now，减指定参数分钟的数据
    /// </summary>
    /// <param name="norunMin"></param>
    /// <returns></returns>
    public static IList<AppOnline> GetOnlines(Int32 norunMin = 10)
    {
        var now = DateTime.Now;
        var exp = new WhereExpression();
        exp &= _.UpdateTime <= now.AddMinutes(-norunMin);

        return FindAll(exp);
    }

    /// <summary>根据应用查询</summary>
    /// <param name="appid"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IList<AppOnline> SearchByAppID(Int32 appid, PageParameter p)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appid);

        return FindAll(_.AppID == appid, p);
    }
    #endregion

    #region 业务操作
    /// <summary>转模型类</summary>
    /// <returns></returns>
    public PeerModel ToModel()
    {
        return new PeerModel
        {
            Instance = Instance,
            Client = Client,
            Machine = Name,
            Version = Version,

            CreateTime = CreateTime,
            UpdateTime = UpdateTime,
        };
    }
    #endregion
}