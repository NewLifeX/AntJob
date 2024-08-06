using System;
using AntJob.Data;
using Xunit;

namespace AntTest;

public class TimeExpressionTests
{
    [Theory]
    [InlineData("+1y", "y", 1)]
    [InlineData("-1y", "y", -1)]
    [InlineData("+5M", "M", 5)]
    [InlineData("-5M", "M", -5)]
    [InlineData("+5d", "d", 5)]
    [InlineData("-5d", "d", -5)]
    [InlineData("-0d", "d", 0)]
    [InlineData("+5H", "H", 5)]
    [InlineData("-5H", "H", -5)]
    [InlineData("+5m", "m", 5)]
    [InlineData("-5m", "m", -5)]
    [InlineData("+5w", "w", 5)]
    [InlineData("-5w", "w", -5)]
    public void ParseItem(String str, String level, Int32 value)
    {
        var item = new TimeExpressionItem();
        var rs = item.Parse(str);
        Assert.True(rs);
        Assert.Equal(level, item.Level);
        Assert.Equal(value, item.Value);

        var time = DateTime.Now;
        var time2 = item.Execute(time);

        if (value > 0)
            Assert.True(time2 > time);
        else if (value < 0)
            Assert.True(time2 < time);
    }

    [Fact]
    public void TestDefault()
    {
        var exp = new TimeExpression("${dt}");
        Assert.Equal("dt", exp.Expression);
        Assert.Equal("dt", exp.VarName);
        Assert.Null(exp.Format);
        Assert.Single(exp.Items);

        var time = DateTime.Now;
        var time2 = exp.Execute(time);
        Assert.Equal(time.Date, time2);

        var rs = exp.Build(time);
        Assert.Equal(time.ToString("yyyyMMdd"), rs);
    }

    [Fact]
    public void Test2()
    {
        var exp = new TimeExpression("${dt+2d}");
        Assert.Equal("dt+2d", exp.Expression);
        Assert.Equal("dt", exp.VarName);
        Assert.Null(exp.Format);
        Assert.Single(exp.Items);

        var time = DateTime.Now;
        var time2 = exp.Execute(time);
        var time3 = time.Date.AddDays(2);
        Assert.Equal(time3, time2);

        var rs = exp.Build(time);
        Assert.Equal(time3.ToString("yyyyMMdd"), rs);
    }

    [Fact]
    public void Test3()
    {
        var exp = new TimeExpression("${dt-3H:yyMMddHH}");
        Assert.Equal("dt-3H:yyMMddHH", exp.Expression);
        Assert.Equal("dt", exp.VarName);
        Assert.Equal("yyMMddHH", exp.Format);
        Assert.Single(exp.Items);

        var time = DateTime.Now;
        var time2 = exp.Execute(time);
        var time3 = time.Date.AddHours(time.Hour - 3);
        Assert.Equal(time3, time2);

        var rs = exp.Build(time);
        Assert.Equal(time3.ToString("yyMMddHH"), rs);
    }

    [Fact]
    public void Test4()
    {
        var exp = new TimeExpression("${dt+1M+4d:yy-MM-dd}");
        Assert.Equal("dt+1M+4d:yy-MM-dd", exp.Expression);
        Assert.Equal("dt", exp.VarName);
        Assert.Equal("yy-MM-dd", exp.Format);
        Assert.Equal(2, exp.Items.Count);

        var time = DateTime.Now;
        var time2 = exp.Execute(time);
        var time3 = time.Date.AddDays(1 - time.Day).AddMonths(1).AddDays(4);
        Assert.Equal(time3, time2);

        var rs = exp.Build(time);
        Assert.Equal(time3.ToString("yy-MM-dd"), rs);
    }
}
