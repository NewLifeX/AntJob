using System;
using System.Collections.Generic;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;

namespace AntJob.Data.Entity;

/// <summary>应用历史</summary>
public partial class AppHistory : EntityBase<AppHistory>
{
    #region 对象操作
    static AppHistory()
    {
        Meta.Table.DataTable.InsertOnly = true;

        // 累加字段
        //var df = Meta.Factory.AdditionalFields;
        //df.Add(__.AppID);

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TraceModule>();
    }

    /// <summary>验证数据</summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public override Boolean Valid(DataMethod method)
    {
        this.TrimExtraLong(__.Remark);

        return base.Valid(method);
    }
    #endregion

    #region 扩展属性
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static AppHistory FindById(Int64 id)
    {
        if (id <= 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据应用、操作查找</summary>
    /// <param name="appId">应用</param>
    /// <param name="action">操作</param>
    /// <returns>实体列表</returns>
    public static IList<AppHistory> FindAllByAppIDAndAction(Int32 appId, String action)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appId && e.Action.EqualIgnoreCase(action));

        return FindAll(_.AppID == appId & _.Action == action);
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

        exp &= _.Id.Between(start, end, Meta.Factory.Snow);
        //exp &= _.CreateTime.Between(start, end);

        if (!key.IsNullOrEmpty())
        {
            exp &= (_.Name.Contains(key) | _.Remark.Contains(key) | _.CreateIP.Contains(key));
        }

        return FindAll(exp, page);
    }
    #endregion

    #region 扩展操作
    /// <summary>类别名实体缓存，异步，缓存10分钟</summary>
    static readonly FieldCache<AppHistory> ActionCache = new FieldCache<AppHistory>(__.Action);

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
    public static AppHistory Create(App app, String action, Boolean success, String remark, String creator, String ip)
    {
        app ??= new App();

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