using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntJob.Extensions
{
    /// <summary>Sql片段。解析sql语句集合</summary>
    public class SqlSection
    {
        #region 属性
        /// <summary>连接名</summary>
        public String ConnName { get; set; }

        /// <summary>操作。添删改查等</summary>
        public String Action { get; set; }

        /// <summary>Sql语句</summary>
        public String Sql { get; set; }
        #endregion

        #region 方法
        /// <summary>分析sql语句集合，得到片段集合，以双换行分隔</summary>
        /// <param name="sqls"></param>
        /// <returns></returns>
        public static SqlSection[] ParseAll(String sqls)
        {
            var list = new List<SqlSection>();

            // 两个换行隔开片段
            var ss = sqls.Split(new[] { "\r\n\r", "\r\r", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in ss)
            {
                var section = new SqlSection();
                section.Parse(item);

                list.Add(section);
            }

            return list.ToArray();
        }

        /// <summary>分析sql语句到片段</summary>
        /// <param name="sql"></param>
        public void Parse(String sql)
        {
            // 解析出来连接名
            var str = sql.Substring("/*", "*/")?.Trim();
            if (!str.IsNullOrEmpty()) ConnName = str.Substring("use ")?.Trim();

            // 剩下的Sql
            sql = sql.Substring("*/")?.Trim();
            Sql = sql;

            // 猜测操作
            if (Action.IsNullOrEmpty())
            {
                if (sql.StartsWithIgnoreCase("select "))
                    Action = "Query";
                else if (sql.StartsWithIgnoreCase("insert into ", "update ", "delete "))
                    Action = "Execute";
                // 批量插入
                else if (sql.StartsWithIgnoreCase("insert "))
                    Action = "Insert";
                // 默认执行，可能是存储过程
                else
                    Action = "Execute";
            }
        }
        #endregion
    }
}