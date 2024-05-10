using System;
using System.Collections.Generic;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace AntJob.Data.Entity;

/// <summary>作业错误</summary>
public partial class JobError : EntityBase<JobError>
{
    #region 对象操作
    static JobError()
    {
        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add<TraceModule>();
    }

    /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew">是否插入</param>
    public override void Valid(Boolean isNew)
    {
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return;

        // 截断错误信息，避免过长
        var len = _.Message.Length;
        if (!Message.IsNullOrEmpty() && len > 0 && Message.Length > len) Message = Message.Substring(0, len);
    }
    #endregion

    #region 扩展属性
    ///// <summary>作业</summary>
    //[XmlIgnore]
    ////[ScriptIgnore]
    //public Job Job => Extends.Get(nameof(Job), k => Job.FindByID(JobID));

    ///// <summary>作业</summary>
    //[XmlIgnore]
    ////[ScriptIgnore]
    //[DisplayName("作业")]
    //[Map(__.JobID)]
    //public String JobName => Job?.Name;

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
    public static JobError FindByID(Int32 id)
    {
        if (id <= 0) return null;

        return Find(_.ID == id);
    }

    /// <summary>根据应用查找</summary>
    /// <param name="appId">应用</param>
    /// <returns>实体列表</returns>
    public static IList<JobError> FindAllByAppID(Int32 appId)
    {
        if (appId <= 0) return new List<JobError>();

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appId);

        return FindAll(_.AppID == appId);
    }

    /// <summary>根据作业查找</summary>
    /// <param name="jobId">作业</param>
    /// <returns>实体列表</returns>
    public static IList<JobError> FindAllByJobID(Int32 jobId)
    {
        if (jobId <= 0) return new List<JobError>();

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.JobID == jobId);

        return FindAll(_.JobID == jobId);
    }
    #endregion

    #region 高级查询
    /// <summary>
    /// 高级查询
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="jobid"></param>
    /// <param name="client"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<JobError> Search(Int32 appid, Int32 jobid, String client, DateTime start, DateTime end, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (appid > 0) exp &= _.AppID == appid;
        if (jobid > 0) exp &= _.JobID == jobid;
        if (!client.IsNullOrEmpty()) exp &= _.Client == client;
        if (!key.IsNullOrEmpty()) exp &= _.Message.Contains(key);
        exp &= _.DataTime.Between(start, end);

        return FindAll(exp, p);
    }

    //public static IList<JobError> SearchByAppID(Int32 appid, PageParameter p)
    //{
    //    if (appid == 0) return new List<JobError>();

    //    return FindAll(_.AppID == appid, p);
    //}

    //public static IList<JobError> FindAllByJobId(Int32 jobid)
    //{
    //    if (jobid == 0) return new List<JobError>();

    //    return FindAll(_.JobID == jobid);
    //}
    #endregion

    #region 业务操作
    /// <summary>
    /// 根据应用删除错误信息
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static Int32 DeleteByAppId(Int32 appid) => Delete(_.AppID == appid);
    #endregion
}