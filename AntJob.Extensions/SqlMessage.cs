using AntJob.Data;
using AntJob.Handlers;
using AntJob.Providers;
using NewLife;
using NewLife.Data;

namespace AntJob.Extensions;

/// <summary>SQL消息处理器，使用消息匹配SQL语句</summary>
/// <remarks>
/// 应用型处理器，可直接使用
/// </remarks>
public class SqlMessage : MessageHandler
{
    #region 构造
    /// <summary>实例化</summary>
    public SqlMessage() => Topic = "Sql";
    #endregion

    /// <summary>根据解码后的消息执行任务</summary>
    /// <param name="ctx">上下文</param>
    /// <returns></returns>
    public override Int32 Execute(JobContext ctx)
    {
        var msgs = ctx.Data as String[];
        var sqls = Job.Data;
        sqls = TemplateHelper.Build(sqls, msgs);
        // 向调度中心返回解析后的Sql语句
        ctx.Remark = sqls;

        // 分解Sql语句得到片段数组
        var sections = SqlSection.ParseAll(sqls);
        if (sections.Length == 0) return -1;

        // 依次执行Sql片段数组。遇到query时，可能需要生产消息
        var rs = SqlHandler.ExecuteSql(sections, ctx, (section, dt) => ProduceMessage(dt, ctx));

        return rs;
    }

    /// <summary>根据查询结果生产消息</summary>
    /// <param name="dt"></param>
    /// <param name="ctx"></param>
    public static void ProduceMessage(DbTable dt, JobContext ctx)
    {
        if (dt == null || dt.Columns == null || dt.Columns.Length == 0 || dt.Rows == null || dt.Rows.Count == 0) return;

        // select id as topic_roleId, id as topic_myId from role where updatetime>='{dt}' and updatetime<'{End}'

        for (var i = 0; i < dt.Columns.Length; i++)
        {
            var col = dt.Columns[i];
            if (col.StartsWithIgnoreCase("topic_"))
            {
                var topic = col.Substring("topic_".Length);
                var messages = dt.Rows.Select(e => e[i] + "").Distinct().ToArray();
                if (messages.Length > 0)
                    ctx.Handler.Produce(topic, messages, new MessageOption { Unique = true });
            }
        }
    }
}