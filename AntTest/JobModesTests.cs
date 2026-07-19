using System;
using System.ComponentModel;
using System.Reflection;
using AntJob.Data;
using Xunit;

namespace AntTest;

/// <summary>作业模式枚举测试</summary>
public class JobModesTests
{
    [Fact]
    public void Data_Value()
    {
        Assert.Equal(1, (Int32)JobModes.Data);
    }

    [Fact]
    public void Time_Value()
    {
        Assert.Equal(2, (Int32)JobModes.Time);
    }

    [Fact]
    public void Message_Value()
    {
        Assert.Equal(3, (Int32)JobModes.Message);
    }

    [Fact]
    public void Data_Description()
    {
        var fi = typeof(JobModes).GetField(nameof(JobModes.Data));
        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("数据调度", attr.Description);
    }

    [Fact]
    public void Time_Description()
    {
        var fi = typeof(JobModes).GetField(nameof(JobModes.Time));
        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("定时调度", attr.Description);
    }

    [Fact]
    public void Message_Description()
    {
        var fi = typeof(JobModes).GetField(nameof(JobModes.Message));
        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("消息调度", attr.Description);
    }
}
