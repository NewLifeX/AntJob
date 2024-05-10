using AntJob.Extensions;
using Xunit;

namespace AntTest
{
    public class SqlSectionTests
    {
        [Fact]
        public void ParseQuery()
        {
            var tt = @"/*use his*/
select * from t1 where time between '{dt}' and '{End}'
";

            var section = new SqlSection();
            section.Parse(tt);

            Assert.Equal("his", section.ConnName);
            Assert.Equal(SqlActions.Query, section.Action);
            Assert.Equal("select * from t1 where time between '{dt}' and '{End}'", section.Sql);
        }

        [Fact]
        public void ParseInsert()
        {
            var tt = @"/*use his*/
insert into t1 (c1, c2) values(v1, v2);
";

            var section = new SqlSection();
            section.Parse(tt);

            Assert.Equal("his", section.ConnName);
            Assert.Equal(SqlActions.Execute, section.Action);
            Assert.Equal("insert into t1 (c1, c2) values(v1, v2)", section.Sql);
        }

        [Fact]
        public void ParseDelete()
        {
            var tt = @"/*use his*/
delete from t2 where time between '{dt}' and '{End}';
";

            var section = new SqlSection();
            section.Parse(tt);

            Assert.Equal("his", section.ConnName);
            Assert.Equal(SqlActions.Execute, section.Action);
            Assert.Equal("delete from t2 where time between '{dt}' and '{End}'", section.Sql);
        }

        [Fact]
        public void ParseUpdate()
        {
            var tt = @"/*use his*/
update t1 set c1=v1, c2=v2 where id=123;
";

            var section = new SqlSection();
            section.Parse(tt);

            Assert.Equal("his", section.ConnName);
            Assert.Equal(SqlActions.Execute, section.Action);
            Assert.Equal("update t1 set c1=v1, c2=v2 where id=123", section.Sql);
        }

        [Fact]
        public void ParseBatchInsert()
        {
            var tt = @"/*use his_bak*/
insert t2;

";

            var section = new SqlSection();
            section.Parse(tt);

            Assert.Equal("his_bak", section.ConnName);
            Assert.Equal(SqlActions.Insert, section.Action);
            Assert.Equal("insert t2", section.Sql);
        }

        [Fact]
        public void ParseAllSqls()
        {
            var tt = @"/*use his*/
select * from t1 where time between '{dt}' and '{End}'

/*use his_bak*/
delete from t2 where time between '{dt}' and '{End}';

/*use his_bak*/
insert t2;
";

            var cs = SqlSection.ParseAll(tt);
            Assert.NotNull(cs);
            Assert.Equal(3, cs.Length);

            Assert.Equal("his", cs[0].ConnName);
            Assert.Equal(SqlActions.Query, cs[0].Action);
            Assert.Equal("select * from t1 where time between '{dt}' and '{End}'", cs[0].Sql);

            Assert.Equal("his_bak", cs[1].ConnName);
            Assert.Equal(SqlActions.Execute, cs[1].Action);
            Assert.Equal("delete from t2 where time between '{dt}' and '{End}'", cs[1].Sql);

            Assert.Equal("his_bak", cs[2].ConnName);
            Assert.Equal(SqlActions.Insert, cs[2].Action);
            Assert.Equal("insert t2", cs[2].Sql);
        }
    }
}
