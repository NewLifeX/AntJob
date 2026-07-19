using System;
using AntJob.Providers;
using Xunit;

namespace AntTest;

/// <summary>消息选项测试</summary>
public class MessageOptionTests
{
    [Fact]
    public void Default_DelayTime_Zero()
    {
        var opt = new MessageOption();
        Assert.Equal(0, opt.DelayTime);
    }

    [Fact]
    public void Default_Unique_False()
    {
        var opt = new MessageOption();
        Assert.False(opt.Unique);
    }

    [Fact]
    public void Default_AppId_Null()
    {
        var opt = new MessageOption();
        Assert.Null(opt.AppId);
    }

    [Fact]
    public void Set_DelayTime()
    {
        var opt = new MessageOption { DelayTime = 60, Unique = true, AppId = "test" };
        Assert.Equal(60, opt.DelayTime);
        Assert.True(opt.Unique);
        Assert.Equal("test", opt.AppId);
    }

    [Fact]
    public void DelayTime_Max()
    {
        var opt = new MessageOption { DelayTime = Int32.MaxValue };
        Assert.Equal(Int32.MaxValue, opt.DelayTime);
    }
}
