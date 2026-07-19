using System;
using AntJob.Data;
using Xunit;

namespace AntTest;

/// <summary>作业状态枚举测试</summary>
public class JobStatusTests
{
    [Fact]
    public void Status_就绪_Value() => Assert.Equal(0, (Int32)JobStatus.就绪);

    [Fact]
    public void Status_抽取中_Value() => Assert.Equal(1, (Int32)JobStatus.抽取中);

    [Fact]
    public void Status_处理中_Value() => Assert.Equal(2, (Int32)JobStatus.处理中);

    [Fact]
    public void Status_错误_Value() => Assert.Equal(3, (Int32)JobStatus.错误);

    [Fact]
    public void Status_完成_Value() => Assert.Equal(4, (Int32)JobStatus.完成);

    [Fact]
    public void Status_取消_Value() => Assert.Equal(5, (Int32)JobStatus.取消);

    [Fact]
    public void Status_延迟_Value() => Assert.Equal(6, (Int32)JobStatus.延迟);

    [Fact]
    public void Status_Values_Consecutive()
    {
        Assert.Equal(0, (Int32)JobStatus.就绪);
        Assert.Equal(1, (Int32)JobStatus.抽取中);
        Assert.Equal(2, (Int32)JobStatus.处理中);
        Assert.Equal(3, (Int32)JobStatus.错误);
        Assert.Equal(4, (Int32)JobStatus.完成);
        Assert.Equal(5, (Int32)JobStatus.取消);
        Assert.Equal(6, (Int32)JobStatus.延迟);
    }
}
