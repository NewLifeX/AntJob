using System.Collections;
using AntJob.Data;
using NewLife.Collections;
using NewLife.Data;

namespace AntJob;

/// <summary>作业上下文</summary>
public class JobContext : IExtend
{
    #region 属性
    /// <summary>作业</summary>
    public Handler Handler { get; set; }

    /// <summary>任务参数</summary>
    public ITask Task { get; set; }

    /// <summary>任务结果</summary>
    public ITaskResult Result { get; set; }

    /// <summary>状态</summary>
    public JobStatus Status { get; set; }

    /// <summary>列表数据</summary>
    public Object Data { get; set; }

    /// <summary>处理总数</summary>
    public Int32 Total { get; set; }

    /// <summary>成功处理数</summary>
    public Int32 Success { get; set; }

    /// <summary>总耗时，毫秒</summary>
    public Double Cost { get; set; }

    /// <summary>最后处理键值。由业务决定，便于分析问题</summary>
    public String Key { get; set; }

    /// <summary>处理异常</summary>
    public Exception Error { get; set; }

    /// <summary>下一次执行时间</summary>
    public DateTime NextTime { get; set; }

    /// <summary>任务备注消息。可用于保存到任务项内容字段</summary>
    public String Remark { get; set; }
    #endregion

    #region 索引器
    /// <summary>用户数据</summary>
    public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

    /// <summary>用户数据</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Object this[String item] { get => Items[item]; set => Items[item] = value; }
    #endregion

    #region 扩展属性
    /// <summary>处理速度</summary>
    public Int32 Speed => (Cost <= 0 || Total == 0) ? 0 : (Int32)Math.Min(Total * 1000L / Cost, Int32.MaxValue);
    #endregion

    #region 方法
    /// <summary>根据指定实体类型返回数据列表</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IList<T> GetDatas<T>()
    {
        if (Data == null) return null;
        if (Data is IList<T> data) return data;

        // 修改列表类型，由 IList<IEntity> 改为 IList<TEntity> ，方便用户使用
        if (Data is IEnumerable enumerable) return enumerable.Cast<T>().ToList();

        return null;
    }
    #endregion
}