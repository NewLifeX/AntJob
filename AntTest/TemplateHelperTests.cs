using System;
using AntJob.Data;
using Xunit;

namespace AntTest
{
    public class TemplateHelperTests
    {
        [Fact]
        public void BuildTest()
        {
            var tt = @"/*use His*/
insert into t1(xxx) select * from t2 where time between {Start} and {End}";
            var start = DateTime.Now;
            var end = start.AddSeconds(30);

            var str = TemplateHelper.Build(tt, start, end);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.DoesNotContain("{Start}", str);
            Assert.DoesNotContain("{End}", str);

            var rs = tt.Replace("{Start}", start.ToFullString()).Replace("{End}", end.ToFullString());
            Assert.Equal(rs, str);
        }

        [Fact]
        public void BuildTest2()
        {
            var tt = @"/*use His*/
insert into t1(xxx) select * from t2 where time between {Start:yyMMdd} and {End:HH:mm:ss}";
            var start = DateTime.Now;
            var end = start.AddSeconds(30);

            var str = TemplateHelper.Build(tt, start, end);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.DoesNotContain("{Start:yyMMdd}", str);
            Assert.DoesNotContain("{End:HH:mm:ss}", str);

            var rs = tt.Replace("{Start:yyMMdd}", start.ToString("yyMMdd")).Replace("{End:HH:mm:ss}", end.ToString("HH:mm:ss"));
            Assert.Equal(rs, str);
        }

        [Fact]
        public void BuildTest3()
        {
            var tt = @"/*use His*/
insert into t1(xxx) select * from t2 where time between {Start:yyMMdd} and {End:HH:mm:ss} time2 between {Start:yyMMdd} and {End:yyMMdd}";
            var start = DateTime.Now;
            var end = start.AddSeconds(30);

            var str = TemplateHelper.Build(tt, start, end);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.DoesNotContain("{Start:yyMMdd}", str);
            Assert.DoesNotContain("{End:HH:mm:ss}", str);
            Assert.DoesNotContain("{End:yyMMdd}", str);

            var rs = tt
                .Replace("{Start:yyMMdd}", start.ToString("yyMMdd"))
                .Replace("{End:HH:mm:ss}", end.ToString("HH:mm:ss"))
                .Replace("{End:yyMMdd}", end.ToString("yyMMdd"))
               ;
            Assert.Equal(rs, str);
        }

        [Fact]
        public void BuildTest4()
        {
            var tt = @"/*use his*/
select * from t1 where time between '{Start}' and '{End}'

/*use hist_bak*/
delete from t2 where time between '{Start}' and '{End}';

/*use hist_bak*/
/*batchinsert t2*/
";
            var start = DateTime.Now;
            var end = start.AddSeconds(30);

            var str = TemplateHelper.Build(tt, start, end);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.DoesNotContain("{Start}", str);
            Assert.DoesNotContain("{End}", str);

            var rs = tt
                .Replace("{Start}", start.ToFullString())
                .Replace("{End}", end.ToFullString())
               ;
            Assert.Equal(rs, str);
        }
    }
}