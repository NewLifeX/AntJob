using System;
using AntJob;
using AntJob.Data;
using Xunit;

namespace AntTest;

/// <summary>任务上下文测试</summary>
public class JobContextTests
{
    [Fact]
    public void Constructor_Default()
    {
        var ctx = new JobContext();
        Assert.Null(ctx.Handler);
        Assert.Null(ctx.Task);
        Assert.Null(ctx.Result);
        Assert.Equal(JobStatus.就绪, ctx.Status);
        Assert.Null(ctx.Data);
        Assert.Equal(0, ctx.Total);
        Assert.Equal(0, ctx.Success);
        Assert.Equal(0, ctx.Cost);
        Assert.Null(ctx.Error);
        Assert.Null(ctx.Key);
        Assert.Null(ctx.Remark);
    }

    [Fact]
    public void SetProperties()
    {
        var ctx = new JobContext
        {
            Total = 100,
            Success = 95,
            Cost = 500,
            Key = "last-key",
            Remark = "完成",
            Status = JobStatus.完成,
        };

        Assert.Equal(100, ctx.Total);
        Assert.Equal(95, ctx.Success);
        Assert.Equal(500, ctx.Cost);
        Assert.Equal("last-key", ctx.Key);
        Assert.Equal("完成", ctx.Remark);
        Assert.Equal(JobStatus.完成, ctx.Status);
    }

    [Fact]
    public void Items_Indexer()
    {
        var ctx = new JobContext();
        ctx["user"] = "alice";
        ctx["count"] = 42;

        Assert.Equal("alice", ctx["user"]);
        Assert.Equal(42, ctx["count"]);
    }

    [Fact]
    public void Speed_Zero_When_NoData()
    {
        var ctx = new JobContext();
        Assert.Equal(0, ctx.Speed);
    }

    [Fact]
    public void Speed_Calculated()
    {
        var ctx = new JobContext { Total = 1000, Cost = 500 };
        // Speed = Total * 1000 / Cost = 1000 * 1000 / 500 = 2000
        Assert.Equal(2000, ctx.Speed);
    }

    [Fact]
    public void Speed_Zero_When_CostZero()
    {
        var ctx = new JobContext { Total = 100, Cost = 0 };
        Assert.Equal(0, ctx.Speed);
    }
}
