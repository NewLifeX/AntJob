using System;
using AntJob.Data.Entity;
using Xunit;

namespace AntTest;

public class JobTests
{
    [Theory]
    [InlineData("09:00-12:00", "2024-01-01 10:00", true)]  // 区间内
    [InlineData("09:00-12:00", "2024-01-01 08:59", false)] // 区间外
    [InlineData("09:00-12:00", "2024-01-01 12:00", false)] // 上界不含
    public void CheckQuiet_SinglePeriod_SameDay(String quietTime, String time, Boolean expected)
    {
        var job = new Job { QuietTime = quietTime };
        var dt = DateTime.Parse(time);
        Assert.Equal(expected, job.CheckQuiet(dt));
    }

    [Theory]
    [InlineData("23:00-02:00", "2024-01-01 23:30", true)]  // 跨天，前一天区间内
    [InlineData("23:00-02:00", "2024-01-02 01:30", true)]  // 跨天，后一天区间内
    [InlineData("23:00-02:00", "2024-01-01 22:59", false)] // 区间外
    [InlineData("23:00-02:00", "2024-01-02 02:00", false)] // 上界不含
    public void CheckQuiet_SinglePeriod_CrossDay(String quietTime, String time, Boolean expected)
    {
        var job = new Job { QuietTime = quietTime };
        var dt = DateTime.Parse(time);
        Assert.Equal(expected, job.CheckQuiet(dt));
    }

    [Theory]
    [InlineData("09:00-12:00,13:00-18:00", "2024-01-01 10:00", true)]  // 第一个区间内
    [InlineData("09:00-12:00,13:00-18:00", "2024-01-01 14:00", true)]  // 第二个区间内
    [InlineData("09:00-12:00,13:00-18:00", "2024-01-01 12:30", false)] // 两区间外
    public void CheckQuiet_MultiPeriod_SameDay(String quietTime, String time, Boolean expected)
    {
        var job = new Job { QuietTime = quietTime };
        var dt = DateTime.Parse(time);
        Assert.Equal(expected, job.CheckQuiet(dt));
    }

    [Theory]
    [InlineData("23:00-02:00,09:00-12:00", "2024-01-01 23:30", true)]  // 跨天区间内
    [InlineData("23:00-02:00,09:00-12:00", "2024-01-02 01:30", true)]  // 跨天区间内
    [InlineData("23:00-02:00,09:00-12:00", "2024-01-01 10:00", true)]  // 当天区间内
    [InlineData("23:00-02:00,09:00-12:00", "2024-01-01 08:00", false)] // 所有区间外
    public void CheckQuiet_MultiPeriod_CrossDay(String quietTime, String time, Boolean expected)
    {
        var job = new Job { QuietTime = quietTime };
        var dt = DateTime.Parse(time);
        Assert.Equal(expected, job.CheckQuiet(dt));
    }

    [Theory]
    [InlineData("23:00-02:00", "2024-01-01 01:00", true)]  // 跨天，凌晨，属于前一天的免打扰
    [InlineData("23:00-02:00", "2024-01-02 01:00", true)]  // 跨天，凌晨，属于前一天的免打扰
    [InlineData("23:00-02:00", "2024-01-01 02:00", false)] // 上界不含
    public void CheckQuiet_CrossDay_EdgeCases(String quietTime, String time, Boolean expected)
    {
        var job = new Job { QuietTime = quietTime };
        var dt = DateTime.Parse(time);
        Assert.Equal(expected, job.CheckQuiet(dt));
    }
}
