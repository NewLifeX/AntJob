using System.Collections;
using AntJob.Data;
using NewLife;
using NewLife.Log;
using XCode;
using XCode.Configuration;

namespace AntJob.Extensions;

/// <summary>数据处理作业（泛型）</summary>
/// <remarks>
/// 定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑。
/// 任务切片条件：StartTime + Step + Offset &lt;= Now
/// </remarks>
/// <typeparam name="TEntity"></typeparam>
public abstract class DataHandler<TEntity> : DataHandler where TEntity : Entity<TEntity>, new()
{
    /// <summary>实例化数据处理作业</summary>
    public DataHandler() => Factory = Entity<TEntity>.Meta.Factory;

    #region 数据处理
    /// <summary>处理一批数据</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    protected override Int32 Execute(JobContext ctx)
    {
        var count = 0;
        foreach (var item in ctx.Data as IEnumerable)
            if (ProcessItem(ctx, item as TEntity)) count++;

        return count;
    }

    /// <summary>处理一个数据对象</summary>
    /// <param name="ctx">上下文</param>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Boolean ProcessItem(JobContext ctx, TEntity entity) => true;
    #endregion
}

/// <summary>数据处理作业</summary>
/// <remarks>
/// 定时调度只要达到时间片开头就可以跑，数据调度要求达到时间片末尾才可以跑。
/// 任务切片条件：StartTime + Step + Offset &lt;= Now
/// </remarks>
public abstract class DataHandler : Handler
{
    #region 属性
    /// <summary>实体工厂</summary>
    public IEntityFactory Factory { get; set; }

    /// <summary>附加条件</summary>
    public String Where { get; set; }

    /// <summary>时间字段 或 雪花Id</summary>
    public FieldItem Field { get; set; }

    /// <summary>排序</summary>
    public String OrderBy { get; set; }

    /// <summary>选择列</summary>
    public String Selects { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化数据库处理器</summary>
    public DataHandler() => Mode = JobModes.Data;
    #endregion

    #region 方法
    /// <summary>开始</summary>
    public override Boolean Start()
    {
        if (Active) return false;

        if (Factory == null) throw new ArgumentNullException(nameof(Factory));

        // 自动识别雪花Id字段
        if (Field == null)
        {
            var pks = Factory.Table.PrimaryKeys;
            if (pks != null && pks.Length == 1 && pks[0].Type == typeof(Int64)) Field = pks[0];
        }
        Field ??= Factory.MasterTime;
        if (Field == null) throw new ArgumentNullException(nameof(Field));

        var job = Job;
        if (job.Step == 0) job.Step = 30;

        // 获取最小时间
        if (job.DataTime.Year < 2000) throw new InvalidOperationException("数据任务必须设置开始时间");

        return base.Start();
    }
    #endregion

    #region 数据处理
    /// <summary>处理任务。内部分批处理</summary>
    /// <param name="ctx"></param>
    protected override void OnProcess(JobContext ctx)
    {
        var prov = Provider;
        var span = DefaultSpan.Current;
        var row = 0;
        while (true)
        {
            ctx.Data = null;
            //ctx.Entity = null;
            ctx.Error = null;

            // 分批抽取
            var data = Fetch(ctx, ref row);

            var list = data as IList;
            if (list != null)
            {
                ctx.Total += list.Count;
                if (span != null) span.Value = ctx.Total;
            }
            ctx.Data = data;

            if (data == null || list != null && list.Count == 0) break;

            // 报告进度
            ctx.Status = JobStatus.处理中;
            prov?.Report(ctx);

            // 批量处理
            ctx.Success += Execute(ctx);

            // 报告进度
            ctx.Status = JobStatus.抽取中;

            // 不满一批，结束
            if (list != null && list.Count < ctx.Task.BatchSize) break;
        }
    }

    /// <summary>分批抽取数据，一个任务内多次调用</summary>
    /// <param name="ctx">上下文</param>
    /// <param name="row">开始行数</param>
    /// <returns></returns>
    protected virtual Object Fetch(JobContext ctx, ref Int32 row)
    {
        var task = ctx.Task;
        if (task == null) throw new ArgumentNullException(nameof(task), "没有设置数据抽取配置");

        // 验证时间段
        var start = task.DataTime;
        var end = task.End;

        // 区间无效
        if (start >= end) return null;

        // 分批获取数据，如果没有取到，则结束
        var fi = Field;
        var exp = new WhereExpression();
        if (fi.Type == typeof(DateTime))
        {
            //因为 XCode 不处理时区，所以这里需要转为本地时间
            if (start > DateTime.MinValue && start < DateTime.MaxValue) exp &= fi >= start.ToLocalTime();
            if (end > DateTime.MinValue && end < DateTime.MaxValue) exp &= fi < end.ToLocalTime();
        }
        else if (fi.Type == typeof(Int64))
        {
            var snow = Factory.Snow;
            if (start > DateTime.MinValue && start < DateTime.MaxValue) exp &= fi >= snow.GetId(start);
            if (end > DateTime.MinValue && end < DateTime.MaxValue) exp &= fi < snow.GetId(end);
        }
        else
            throw new NotSupportedException($"不支持抽取[{fi.Type.FullName}]类型的字段数据！");

        if (!Where.IsNullOrEmpty()) exp &= Where;

        var list = Factory.FindAll(exp, OrderBy, Selects, row, task.BatchSize);

        // 取到数据，需要滑动窗口
        if (list.Count > 0) row += list.Count;

        return list;
    }

    /// <summary>处理一批数据</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    protected override Int32 Execute(JobContext ctx)
    {
        var count = 0;
        foreach (var item in ctx.Data as IEnumerable)
        {
            if (ProcessItem(ctx, item as IEntity)) count++;
        }

        return count;
    }

    /// <summary>处理一个数据对象</summary>
    /// <param name="ctx">上下文</param>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Boolean ProcessItem(JobContext ctx, IEntity entity) => true;
    #endregion
}