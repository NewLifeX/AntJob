using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntJob.Data;
using XCode.DataAccessLayer;

namespace AntJob.Extensions
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
            Mode = JobModes.Sql;

            var job = Job;
            job.BatchSize = 8;
        }
        #endregion

        /// <summary>执行</summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        protected override Int32 Execute(JobContext ctx)
        {
            var sql = ctx.Task.Data as String;
            if (sql.IsNullOrWhiteSpace()) return -1;

            // 解析出来连接名
            var connName = "";
            var str = sql.Substring("/*", "*/")?.Trim();
            if (!str.IsNullOrEmpty()) connName = str.Substring("use ")?.Trim();
            if (connName.IsNullOrEmpty()) throw new InvalidDataException("无法解析连接名");

            // 剩下的Sql
            sql = sql.Substring("*/")?.Trim();
            if (sql.IsNullOrWhiteSpace()) return -1;

            var dal = DAL.Create(connName);
            var rs = dal.Execute(sql);

            return rs;
        }
    }
}