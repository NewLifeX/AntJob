using NewLife;
using NewLife.Data;
using NewLife.Model;
using XCode.DataAccessLayer;

namespace AntJob.Extensions;

/// <summary>Sql操作</summary>
public enum SqlActions
{
    /// <summary>查询。返回数据集</summary>
    Query,

    /// <summary>执行Sql或存储过程。返回影响行数</summary>
    Execute,

    /// <summary>插入。批量插入数据，需要传入数据集</summary>
    Insert,
}

/// <summary>Sql片段。解析sql语句集合</summary>
public class SqlSection
{
    #region 属性
    /// <summary>连接名</summary>
    public String ConnName { get; set; }

    /// <summary>操作。添删改查等</summary>
    public SqlActions Action { get; set; }

    /// <summary>Sql语句</summary>
    public String Sql { get; set; }
    #endregion

    #region 解析
    /// <summary>分析sql语句集合，得到片段集合，以双换行分隔</summary>
    /// <param name="sqls"></param>
    /// <returns></returns>
    public static SqlSection[] ParseAll(String sqls)
    {
        var list = new List<SqlSection>();
        if (sqls.IsNullOrEmpty()) return list.ToArray();

        // 两个换行隔开片段
        var ss = sqls.Split(new[] { "\r\n\r", "\r\r", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var connName = "";
        foreach (var item in ss)
        {
            var section = new SqlSection();
            section.Parse(item);

            // 如果当前片段未指定连接名，则使用上一个
            if (section.ConnName.IsNullOrEmpty())
                section.ConnName = connName;
            else
                connName = section.ConnName;

            if (section.ConnName.IsNullOrEmpty()) throw new Exception("未指定连接名！");

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
        sql = sql.Substring("*/")?.Trim()?.Trim(';')?.Trim();
        Sql = sql;

        // 猜测操作
        if (sql.StartsWithIgnoreCase("select "))
            Action = SqlActions.Query;
        else if (sql.StartsWithIgnoreCase("insert into ", "update ", "delete "))
            Action = SqlActions.Execute;
        // 批量插入
        else if (sql.StartsWithIgnoreCase("insert "))
            Action = SqlActions.Insert;
        // 默认执行，可能是存储过程
        else
            Action = SqlActions.Execute;
    }
    #endregion

    #region 执行处理
    /// <summary>查询数据集</summary>
    /// <returns></returns>
    public DbTable Query() => DAL.Create(ConnName).Query(Sql);

    /// <summary>执行</summary>
    /// <returns></returns>
    public Int32 Execute()
    {
        var dal = DAL.Create(ConnName);

        // 解析数据表，如果目标表不存在，则返回
        var tableName = "";
        if (Sql.StartsWithIgnoreCase("delete "))
            tableName = Sql.Substring(" from ", " ")?.Trim();
        else if (Sql.StartsWithIgnoreCase("udpate "))
            tableName = Sql.Substring("udpate ", " ")?.Trim();

        if (!tableName.IsNullOrEmpty())
        {
            var table = dal.Tables?.FirstOrDefault(e => e.TableName.EqualIgnoreCase(tableName));
            if (table == null) return -1;
        }

        return dal.Execute(Sql);
    }

    /// <summary>批量插入</summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public Int32 BatchInsert(DbTable dt)
    {
        var tableName = Sql.Substring(" ")?.Trim(';');
        var dal = DAL.Create(ConnName);

        // 执行反向工程，该建表就建表
        //dal.CheckDatabase();

        var table = dal.Tables?.FirstOrDefault(e => e.TableName.EqualIgnoreCase(tableName));
        //if (table == null) throw new Exception($"在连接[{ConnName}]中无法找到数据表[{tableName}]");
        if (table == null)
        {
            var ioc = ObjectContainer.Current;
            table = ioc.Resolve<IDataTable>();
            table.TableName = tableName;

            for (var i = 0; i < dt.Columns.Length; i++)
            {
                var dc = table.CreateColumn();
                dc.ColumnName = dt.Columns[i];
                dc.DataType = dt.Types[i];

                table.Columns.Add(dc);
            }

            dal.SetTables(table);
        }

        // 选取目标表和数据集共有的字段
        tableName = dal.Db.FormatName(table);
        var columns = new List<IDataColumn>();
        foreach (var dc in table.Columns)
        {
            if (dc.ColumnName.EqualIgnoreCase(dt.Columns)) columns.Add(dc);
        }

        return dal.Session.Insert(table, columns.ToArray(), dt.Cast<IModel>());
    }
    #endregion
}