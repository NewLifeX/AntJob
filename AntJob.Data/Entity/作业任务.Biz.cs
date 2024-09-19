using System;
using System.Collections.Generic;
using System.Linq;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace AntJob.Data.Entity;

/// <summary>作业任务</summary>
public partial class JobTask : EntityBase<JobTask>
{
    #region 对象操作
    static JobTask()
    {
        // 累加字段
        var df = Meta.Factory.AdditionalFields;
        df.Add(__.Error);
        df.Add(__.Times);

        Meta.Modules.Add<IPModule>();
        Meta.Modules.Add<TimeModule>();
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
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static JobTask FindByID(Int64 id)
    {
        if (id <= 0) return null;

        //// 单对象缓存
        //return Meta.SingleCache[id];
        return Find(_.ID == id);
    }

    /// <summary>
    /// 根据应用查询
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static IList<JobTask> FindAllByAppID(Int32 appid)
    {
        if (appid == 0) return new List<JobTask>();

        return FindAll(_.AppID == appid);
    }

    /// <summary>
    /// 根据任务查询个数
    /// </summary>
    /// <param name="jobid"></param>
    /// <returns></returns>
    public static Int32 FindCountByJobId(Int32 jobid) => (Int32)FindCount(_.JobID == jobid);

    /// <summary>查找作业下小于指定创建时间的最后一个任务</summary>
    /// <param name="jobid"></param>
    /// <param name="createTime"></param>
    /// <returns></returns>
    public static JobTask FindLastByJobId(Int32 jobid, DateTime createTime)
    {
        return FindAll(_.JobID == jobid & _.CreateTime < createTime, _.CreateTime.Desc(), null, 0, 1).FirstOrDefault();
    }

    /// <summary>根据应用、客户端、状态查找</summary>
    /// <param name="appId">应用</param>
    /// <param name="client">客户端</param>
    /// <param name="status">状态</param>
    /// <returns>实体列表</returns>
    public static IList<JobTask> FindAllByAppIDAndClientAndStatus(Int32 appId, String client, JobStatus status)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appId && e.Client.EqualIgnoreCase(client) && e.Status == status);

        return FindAll(_.AppID == appId & _.Client == client & _.Status == status);
    }

    /// <summary>根据作业、状态、数据时间查找</summary>
    /// <param name="jobId">作业</param>
    /// <param name="status">状态</param>
    /// <param name="dataTime">数据时间</param>
    /// <returns>实体列表</returns>
    public static IList<JobTask> FindAllByJobIDAndStatusAndDataTime(Int32 jobId, JobStatus status, DateTime dataTime)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.JobID == jobId && e.Status == status && e.DataTime == dataTime);

        return FindAll(_.JobID == jobId & _.Status == status & _.DataTime == dataTime);
    }

    /// <summary>根据作业、数据时间查找</summary>
    /// <param name="jobId">作业</param>
    /// <param name="dataTime">数据时间</param>
    /// <returns>实体列表</returns>
    public static IList<JobTask> FindAllByJobIDAndDataTime(Int32 jobId, DateTime dataTime)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.JobID == jobId && e.DataTime == dataTime);

        return FindAll(_.JobID == jobId & _.DataTime == dataTime);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="id"></param>
    /// <param name="appid"></param>
    /// <param name="jobid"></param>
    /// <param name="status"></param>
    /// <param name="dataStart"></param>
    /// <param name="dataEnd"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="client"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<JobTask> Search(Int32 id, Int32 appid, Int32 jobid, JobStatus status, DateTime dataStart, DateTime dataEnd, DateTime start, DateTime end, String client, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (id > 0) exp &= _.ID == id;
        if (appid >= 0) exp &= _.AppID == appid;
        if (jobid >= 0) exp &= _.JobID == jobid;
        if (status >= JobStatus.就绪) exp &= _.Status == status;
        if (!client.IsNullOrEmpty()) exp &= _.Client == client;
        if (!key.IsNullOrEmpty()) exp &= _.Data.Contains(key) | _.Message.Contains(key) | _.Key == key;

        exp &= _.DataTime.Between(dataStart, dataEnd);
        exp &= _.UpdateTime.Between(start, end);

        return FindAll(exp, p);
    }

    /// <summary>获取该任务下特定状态的任务项</summary>
    /// <param name="taskid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="maxRetry"></param>
    /// <param name="maxError"></param>
    /// <param name="status"></param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public static IList<JobTask> Search(Int32 taskid, DateTime start, DateTime end, Int32 maxRetry, Int32 maxError, JobStatus[] status, Int32 count)
    {
        var exp = new WhereExpression();
        if (taskid > 0) exp &= _.JobID == taskid;
        if (maxRetry > 0) exp &= _.Times < maxRetry;
        if (status != null && status.Length > 0) exp &= _.Status.In(status);

        // 限制任务的错误次数，避免无限执行
        if (maxError > 0) exp &= _.Error < maxError;

        if (start.Year > 2000) exp &= _.UpdateTime >= start;
        if (end.Year > 2000) exp &= _.UpdateTime < end;

        return FindAll(exp, _.UpdateTime.Asc(), null, 0, count);
    }
    #endregion

    #region 业务操作
    /// <summary>重置</summary>
    public void Reset()
    {
        Total = 0;
        Success = 0;
        Status = JobStatus.就绪;

        Save();
    }

    /// <summary>删除该ID及以前的作业项</summary>
    /// <param name="jobid"></param>
    /// <param name="maxid"></param>
    /// <returns></returns>
    public static Int32 DeleteByID(Int32 jobid, Int64 maxid) => maxid <= 0 ? 0 : Delete(_.JobID == jobid & _.ID <= maxid);

    //public static Int32 DeleteByAppId(Int32 appid) => Delete(_.AppID == appid);

    /// <summary>删除作业已不存在的任务</summary>
    /// <returns></returns>
    public static Int32 DeleteNoJob() => Delete(_.JobID.NotIn(Entity.Job.FindSQLWithKey()));

    /// <summary>转模型类</summary>
    /// <returns></returns>
    public TaskModel ToModel()
    {
        return new TaskModel
        {
            ID = ID,

            DataTime = DataTime,
            End = End,
            //Offset = Offset,
            //Step = Step,
            BatchSize = BatchSize,

            Data = Data,
        };
    }
    #endregion
}