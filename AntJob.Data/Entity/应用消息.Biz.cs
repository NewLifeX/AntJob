using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace AntJob.Data.Entity;

/// <summary>应用消息。消息调度，某些作业负责生产消息，供其它作业进行消费处理</summary>
public partial class AppMessage : EntityBase<AppMessage>
{
    #region 对象操作
    static AppMessage()
    {
        // 过滤器 UserModule、TimeModule、IPModule
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
    public static AppMessage FindByID(Int64 id)
    {
        if (id <= 0) return null;

        return Find(_.Id == id);
    }

    /// <summary>
    /// 查询当前应用的消息数
    /// </summary>
    /// <param name="appid"></param>
    /// <returns></returns>
    public static Int32 FindCountByAppID(Int32 appid)
    {
        if (appid == 0) return 0;

        return (Int32)FindCount(_.AppID == appid);
    }

    /// <summary>
    /// 查询当前作业的消息数
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static Int32 FindCountByAppIDAndTopic(Int32 appid, String topic)
    {
        if (appid == 0) return 0;

        return (Int32)FindCount(_.AppID == appid & _.Topic == topic);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="appid"></param>
    /// <param name="jobid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static IEnumerable<AppMessage> Search(Int32 appid, Int32 jobid, DateTime start, DateTime end, String key, PageParameter p)
    {
        var exp = new WhereExpression();

        if (appid > 0) exp &= _.AppID == appid;
        if (jobid > 0) exp &= _.JobID == jobid;
        if (!key.IsNullOrEmpty()) exp &= _.Topic.Contains(key) | _.Data.Contains(key);

        exp &= _.Id.Between(start, end, Meta.Factory.Snow);
        //exp &= _.UpdateTime.Between(start, end);

        return FindAll(exp, p);
    }
    #endregion

    #region 业务操作
    /// <summary>根据应用、主题、以及时间查找一定数量消息</summary>
    /// <param name="appid"></param>
    /// <param name="topic"></param>
    /// <param name="endTime"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IList<AppMessage> GetTopic(Int32 appid, String topic, DateTime endTime, Int32 count)
    {
        return FindAll(_.AppID == appid & _.Topic == topic & _.Id <= Meta.Factory.Snow.GetId(endTime), _.Id.Asc(), null, 0, count);
    }

    /// <summary>去重过滤</summary>
    /// <param name="appid"></param>
    /// <param name="topic"></param>
    /// <param name="messages"></param>
    /// <returns></returns>
    public static String[] Filter(Int32 appid, String topic, String[] messages)
    {
        var list = new List<String>();

        // 分批核查是否存在
        for (var i = 0; i < messages.Length; i += 100)
        {
            var batch = messages.Skip(i).Take(100).ToList();
            var ms = FindAll(_.AppID == appid & _.Topic == topic & _.Data.In(batch));
            // 去掉找到的项
            foreach (var item in ms)
            {
                batch.Remove(item.Data);
            }
            // 剩下的加入结果
            if (batch.Count > 0) list.AddRange(batch);
        }

        return list.ToArray();
    }
    #endregion
}