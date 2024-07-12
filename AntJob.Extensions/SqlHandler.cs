using AntJob.Data;
using NewLife.Data;
using XCode.DataAccessLayer;

namespace AntJob.Extensions;

/// <summary>SQL语句处理器，定时执行SQL语句</summary>
/// <remarks>
/// 应用型处理器，可直接使用
/// </remarks>
public class SqlHandler : Handler
{
    #region 构造
    /// <summary>实例化</summary>
    public SqlHandler()
    {
        //Mode = JobModes.Sql;

        var job = Job;
        job.BatchSize = 8;
    }
    #endregion

    /// <summary>执行</summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public override Int32 Execute(JobContext ctx)
    {
        //var sqls = ctx.Task.Data as String;
        var sqls = Job.Data;
        sqls = TemplateHelper.Build(sqls, ctx.Task.DataTime, ctx.Task.End);
        // 向调度中心返回解析后的Sql语句
        ctx.Remark = sqls;

        var sections = SqlSection.ParseAll(sqls);
        if (sections.Length == 0) return -1;

        var rs = ExecuteSql(sections, ctx, (section, dt) => SqlMessage.ProduceMessage(dt, ctx));

        return rs;
    }

    /// <summary>执行Sql集合</summary>
    /// <param name="sections"></param>
    /// <param name="ctx"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static Int32 ExecuteSql(SqlSection[] sections, JobContext ctx, Action<SqlSection, DbTable> callback = null)
    {
        if (sections == null || sections.Length == 0) return -1;

        var rs = 0;
        ctx.Total = 0;

        // 打开事务
        foreach (var item in sections)
            if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).BeginTransaction();
        try
        {
            // 按顺序执行处理Sql语句
            DbTable dt = null;
            foreach (var section in sections)
                switch (section.Action)
                {
                    case SqlActions.Query:
                        dt = section.Query();
                        if (dt != null) ctx.Total += dt.Rows.Count;

                        // 处理生产消息
                        callback?.Invoke(section, dt);

                        break;
                    case SqlActions.Execute:
                        rs += section.Execute();
                        break;
                    case SqlActions.Insert:
                        if (dt.Rows.Count > 0) rs += section.BatchInsert(dt);
                        break;
                    default:
                        break;
                }

            // 提交事务
            foreach (var item in sections)
                if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).Commit();
        }
        catch
        {
            // 回滚事务
            foreach (var item in sections)
                if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).Rollback();

            throw;
        }

        return rs;
    }
}