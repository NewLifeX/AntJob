using System;
using AntJob.Data;
using AntJob.Extensions;
using AntJob.Handlers;
using NewLife.Data;
using XCode.DataAccessLayer;

namespace AntJob
{
    /// <summary>SQL消息处理器，使用消息匹配SQL语句</summary>
    /// <remarks>
    /// 应用型处理器，可直接使用
    /// </remarks>
    public class SqlMessage : MessageHandler
    {
        #region 构造
        /// <summary>实例化</summary>
        public SqlMessage()
        {
            Topic = "Sql";
            //Mode = JobModes.Message;

            //var job = Job;
            //job.BatchSize = 8;
        }
        #endregion

        /// <summary>根据解码后的消息执行任务</summary>
        /// <param name="ctx">上下文</param>
        /// <returns></returns>
        protected override Int32 Execute(JobContext ctx)
        {
            var msgs = ctx.Data as String[];
            var sqls = Job.Data;
            sqls = TemplateHelper.Build(sqls, msgs);
            // 向调度中心返回解析后的Sql语句
            ctx.Remark = sqls;

            var sections = SqlSection.ParseAll(sqls);
            if (sections.Length == 0) return -1;

            var rs = SqlHandler.ExecuteSql(sections, ctx);

            return rs;
        }
    }
}