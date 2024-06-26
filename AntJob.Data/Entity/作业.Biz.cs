using System;
using System.Collections.Generic;
using NewLife;
using NewLife.Data;
using NewLife.Threading;
using XCode;

namespace AntJob.Data.Entity;

/// <summary>作业</summary>
public partial class Job : EntityBase<Job>
{
    #region 对象操作
    static Job()
    {
        // 累加字段
        var df = Meta.Factory.AdditionalFields;
        df.Add(__.Total);
        df.Add(__.Success);
        df.Add(__.Error);
        df.Add(__.Times);
        //df.Add(__.MessageCount);

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

        //if ((Mode == JobModes.Sql || Mode == JobModes.CSharp) && Data.IsNullOrEmpty())
        //    throw new ArgumentNullException(nameof(Data), $"{Mode}调度模式要求设置Data模板");

        // 参数默认值
        var step = Step;
        if (step == 0) step = Step = 5;
        if (MaxRetain == 0) MaxRetain = 30;
        if (MaxIdle == 0) MaxIdle = GetDefaultIdle();

        if (isNew)
        {
            if (!Dirtys[nameof(MaxRetry)]) MaxRetry = 100;
            if (!Dirtys[nameof(MaxTime)]) MaxTime = 600;
            if (!Dirtys[nameof(ErrorDelay)]) ErrorDelay = 60;
            if (!Dirtys[nameof(MaxIdle)]) MaxIdle = GetDefaultIdle();
        }

        //// 截断错误信息，避免过长
        //var len = _.Remark.Length;
        //if (!Remark.IsNullOrEmpty() && len > 0 && Remark.Length > len) Remark = Remark.Substring(0, len);

        // 定时任务自动生成Cron
        if (Mode == JobModes.Time && Cron.IsNullOrEmpty())
        {
            if (step < 60)
                Cron = $"0 */{step} * * * ?";
            else if (step % 86400 == 0 && step / 86400 < 30)
                Cron = $"0 0 0 0/{step / 86400} * ?";
            else if (step % 3600 == 0 && step / 3600 < 24)
                Cron = $"0 0 0/{step / 3600} * * ?";
            else if (step % 60 == 0 && step / 60 < 60)
                Cron = $"0 0/{step / 60} * * * ?";
        }

        var app = App;
        if (isNew && app != null)
        {
            app.JobCount = FindCountByAppID(app.ID);
            app.SaveAsync();
        }
    }

    private Int32 GetDefaultIdle()
    {
        // 定时调度，取步进加一分钟
        if (Mode == JobModes.Time) return Step + 600;

        return 3600;
    }

    /// <summary>删除</summary>
    /// <returns></returns>
    protected override Int32 OnDelete()
    {
        var rs = base.OnDelete();

        var app = App;
        if (app != null)
        {
            app.JobCount = FindCountByAppID(app.ID);
            app.SaveAsync();
        }

        return rs;
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
    public static Job FindByID(Int32 id)
    {
        if (id <= 0) return null;

        //// 实体缓存
        //if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

        // 单对象缓存
        return Meta.SingleCache[id];
        //return Find(_.ID == id);
    }

    /// <summary>根据应用、名称查找</summary>
    /// <param name="appid">应用</param>
    /// <param name="name">名称</param>
    /// <returns>实体对象</returns>
    public static Job FindByAppIDAndName(Int32 appid, String name)
    {
        //// 实体缓存
        //if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.AppID == appid && e.Name == name);

        return Find(_.AppID == appid & _.Name == name);
    }

    /// <summary>根据应用查询</summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static IList<Job> FindAllByAppID(Int32 appid)
    {
        if (appid == 0) return new List<Job>();

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appid);

        return FindAll(_.AppID == appid);
    }

    /// <summary>
    /// 直接查库，不查缓存
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static IList<Job> FindAllByAppID2(Int32 appid)
    {
        if (appid == 0) return new List<Job>();

        return FindAll(_.AppID == appid);
    }

    /// <summary>
    /// 查询当前应用的作业数
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static Int32 FindCountByAppID(Int32 appid)
    {
        if (appid == 0) return 0;

        return (Int32)FindCount(_.AppID == appid);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="id"></param>
    /// <param name="appid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="mode"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<Job> Search(Int32 id, Int32 appid, DateTime start, DateTime end, Int32 mode, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (id > 0) exp &= _.ID == id;
        if (appid > 0) exp &= _.AppID == appid;
        if (mode > 0) exp &= _.Mode == mode;
        if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key);
        exp &= _.CreateTime.Between(start, end);

        return FindAll(exp, p);
    }
    #endregion

    #region 业务操作
    /// <summary>是否已准备就绪</summary>
    /// <returns></returns>
    public Boolean IsReady()
    {
        switch (Mode)
        {
            case JobModes.Data:
            case JobModes.Time:
                return DataTime.Year > 2000 && Step > 0;
            case JobModes.Message:
                return Topic.IsNullOrEmpty();
            default:
                break;
        }

        return false;
    }

    /// <summary>获取下一次执行时间</summary>
    /// <returns></returns>
    public DateTime GetNext()
    {
        var step = Step;
        if (step <= 0) step = 30;

        switch (Mode)
        {
            case JobModes.Data:
                break;
            case JobModes.Time:
                if (!Cron.IsNullOrEmpty())
                {
                    var time = DataTime.Year > 2000 ? DataTime : DateTime.Now;
                    var next = DateTime.MaxValue;
                    foreach (var item in Cron.Split(";"))
                    {
                        var cron = new Cron(item);
                        var dt = cron.GetNext(time);
                        if (dt < next) next = dt;
                    }
                    return next;
                    //return NewLife.Threading.Cron.GetNext(Cron.Split(";"), time);
                }
                else
                {
                    return DataTime.AddSeconds(step);
                }
            case JobModes.Message:
                break;
            default:
                break;
        }

        return DateTime.MinValue;
    }

    /// <summary>重置任务，让它从新开始工作</summary>
    /// <param name="days">重置到多少天之前</param>
    /// <param name="stime">开始时间（优先级低于days）</param>
    /// <param name="etime">结束时间（优先级低于days）</param>
    public void ResetTime(Int32 days, DateTime stime, DateTime etime)
    {
        if (days < 0)
        {
            //DataTime = DateTime.MinValue;

            if (stime > DateTime.MinValue)
                DataTime = stime;
            End = etime;
        }
        else
            DataTime = DateTime.Now.Date.AddDays(-days);

        Save();
    }

    /// <summary>重置任务，让它从新开始工作</summary>
    public void ResetOther()
    {
        Total = 0;
        Success = 0;
        Times = 0;
        Speed = 0;
        Error = 0;

        Save();
    }

    /// <summary>删除过期</summary>
    /// <returns></returns>
    public Int32 DeleteItems()
    {
        // 每个作业保留1000行
        var count = JobTask.FindCountByJobId(ID);
        if (count <= 1000) return 0;

        var days = MaxRetain;
        if (days <= 0) days = 3;
        var last = JobTask.FindLastByJobId(ID, DateTime.Now.AddDays(-days));
        if (last == null) return 0;

        return JobTask.DeleteByID(ID, last.ID);
    }

    /// <summary>转模型类</summary>
    /// <returns></returns>
    public JobModel ToModel()
    {
        // 如果禁用，仅返回最简单的字段
        // 缺少开始时间赋值，会导致客户端启动校验失败，Job没有启用的状态下服务器报错无法正常启动
        if (!Enable) return new JobModel { Name = Name, Enable = Enable, DataTime = DataTime };

        return new JobModel
        {
            Name = Name,
            ClassName = ClassName,
            Enable = Enable,

            DataTime = DataTime,
            End = End,
            Cron = Cron,
            Topic = Topic,
            Data = Data,

            Offset = Offset,
            Step = Step,
            BatchSize = BatchSize,
            MaxTask = MaxTask,

            Mode = Mode,
        };
    }
    #endregion
}