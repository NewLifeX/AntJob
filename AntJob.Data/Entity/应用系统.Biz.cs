using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;

namespace AntJob.Data.Entity;

/// <summary>应用系统。数据作业隶属于某个应用</summary>
public partial class App : EntityBase<App>
{
    #region 对象操作
    static App()
    {
        // 累加字段
        var df = Meta.Factory.AdditionalFields;
        //df.Add(__.MessageCount);

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<UserModule>();
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<IPModule>();

        // 单对象缓存
        var sc = Meta.SingleCache;
        sc.FindSlaveKeyMethod = k => Find(__.Name, k);
        sc.GetSlaveKeyMethod = e => e.Name;
    }

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew">是否插入</param>
    public override void Valid(Boolean isNew)
    {
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return;

        // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
        if (Name.IsNullOrEmpty()) throw new ArgumentNullException(nameof(Name), "名称不能为空！");

        if (!isNew && JobCount == 0 && !Dirtys[nameof(JobCount)]) JobCount = Job.FindCountByAppID(ID);
        if (!isNew && MessageCount == 0 && !Dirtys[nameof(MessageCount)]) MessageCount = AppMessage.FindCountByAppID(ID);
    }
    #endregion

    #region 扩展属性
    /// <summary>作业集合</summary>
    [XmlIgnore, IgnoreDataMember]
    public IList<Job> Jobs => Extends.Get(nameof(Jobs), k => Job.FindAllByAppID(ID));
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static App FindByID(Int32 id)
    {
        if (id <= 0) return null;

        // 单对象缓存
        return Meta.SingleCache[id];
    }

    /// <summary>根据名称查找</summary>
    /// <param name="name">名称</param>
    /// <returns>实体对象</returns>
    public static App FindByName(String name)
    {
        // 单对象缓存
        return Meta.SingleCache.GetItemWithSlaveKey(name) as App;
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="category"></param>
    /// <param name="enable"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<App> Search(String category, Boolean? enable, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (enable != null) exp &= _.Enable == enable;
        if (!category.IsNullOrEmpty()) exp &= _.Category == category;

        if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key) | _.DisplayName.Contains(key);

        return FindAll(exp, p);
    }
    #endregion

    #region 业务操作
    /// <summary>分类单对象缓存</summary>
    static FieldCache<App> CategoryCache = new FieldCache<App>(__.Category);

    /// <summary>查询所有分类缓存</summary>
    /// <returns></returns>
    public static IDictionary<String, String> FindAllCategoryByCache() => CategoryCache.FindAllName();
    #endregion
}