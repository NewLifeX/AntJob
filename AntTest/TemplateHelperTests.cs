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
    }
}