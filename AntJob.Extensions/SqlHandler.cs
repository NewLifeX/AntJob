using System;
using AntJob.Data;
using AntJob.Extensions;
using NewLife.Data;
using XCode.DataAccessLayer;

namespace AntJob
{
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
        protected override Int32 Execute(JobContext ctx)
        {
            var sqls = ctx.Task.Data as String;
            var sections = SqlSection.ParseAll(sqls);
            if (sections.Length == 0) return -1;

            var rs = 0;
            ctx.Total = 0;

            // 打开事务
            foreach (var item in sections)
            {
                if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).BeginTransaction();
            }
            try
            {
                // 按顺序执行处理Sql语句
                DbTable dt = null;
                foreach (var item in sections)
                {
                    switch (item.Action)
                    {
                        case SqlActions.Query:
                            dt = item.Query();
                            if (dt != null) ctx.Total += dt.Rows.Count;
                            break;
                        case SqlActions.Execute:
                            rs += item.Execute();
                            break;
                        case SqlActions.Insert:
                            rs += item.BatchInsert(dt);
                            break;
                        default:
                            break;
                    }
                }

                // 提交事务
                foreach (var item in sections)
                {
                    if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).Commit();
                }
            }
            catch
            {
                // 回滚事务
                foreach (var item in sections)
                {
                    if (item.Action != SqlActions.Query) DAL.Create(item.ConnName).Rollback();
                }

                throw;
            }

            return rs;
        }
    }
}