using System;
using AntJob;
using AntJob.Data;
using AntJob.Providers;
using Xunit;

namespace AntTest;

/// <summary>测试用处理器，用于 Handler 生命周期测试</summary>
internal class TestHandler : Handler
{
    /// <summary>初始化调用标记</summary>
    public Boolean InitCalled { get; private set; }

    /// <summary>初始化</summary>
    public override void Init()
    {
        base.Init();
        InitCalled = true;
    }

    /// <summary>返回模拟处理成功数</summary>
    public override Int32 Execute(JobContext ctx) => 1;
}

/// <summary>完成上报抛异常的处理器</summary>
internal class FinishThrowsHandler : Handler
{
    public override Int32 Execute(JobContext ctx) => 1;

    protected override void OnFinish(JobContext ctx) => throw new InvalidOperationException("模拟上报失败");
}

/// <summary>处理器基类生命周期测试</summary>
public class HandlerTests
{
    [Fact]
    public void Constructor_Name()
    {
        var handler = new TestHandler();
        Assert.Equal("Test", handler.Name);
    }

    [Fact]
    public void Constructor_Mode()
    {
        var handler = new TestHandler();
        Assert.Equal(JobModes.Time, handler.Mode);
    }

    [Fact]
    public void Constructor_Job()
    {
        var handler = new TestHandler();
        Assert.NotNull(handler.Job);
        Assert.Equal(30, handler.Job.Step);
        Assert.Equal(15, handler.Job.Offset);
        Assert.Equal(JobModes.Time, handler.Job.Mode);
        Assert.Equal(1, handler.Job.MaxTask);
    }

    [Fact]
    public void Constructor_DataTime()
    {
        var handler = new TestHandler();
        Assert.Equal(DateTime.Now.Date, handler.Job.DataTime.Date);
    }

    [Fact]
    public void Init_Called()
    {
        var handler = new TestHandler();
        Assert.False(handler.InitCalled);
        handler.Init();
        Assert.True(handler.InitCalled);
    }

    [Fact]
    public void Init_Time_MaxTask()
    {
        var handler = new TestHandler();
        Assert.Equal(1, handler.Job.MaxTask);
        handler.Init();
        Assert.Equal(1, handler.Job.MaxTask);
    }

    [Fact]
    public void Start_Initial_Active_False()
    {
        var handler = new TestHandler();
        Assert.False(handler.Active);
    }

    [Fact]
    public void Start_Active_True()
    {
        var handler = new TestHandler();
        handler.Start();
        Assert.True(handler.Active);
    }

    [Fact]
    public void Start_Repeat_ReturnsFalse()
    {
        var handler = new TestHandler();
        Assert.True(handler.Start());
        Assert.False(handler.Start());
    }

    [Fact]
    public void Stop_Active_False()
    {
        var handler = new TestHandler();
        handler.Start();
        Assert.True(handler.Active);
        handler.Stop("test");
        Assert.False(handler.Active);
    }

    [Fact]
    public void Execute_Returns_Value()
    {
        var handler = new TestHandler();
        var result = handler.Execute(new JobContext());
        Assert.Equal(1, result);
    }

    [Fact]
    public void Process_Null_ReturnsNull()
    {
        var handler = new TestHandler();
        var ctx = handler.Process(null);
        Assert.Null(ctx);
    }

    [Fact]
    public void Process_ValidTask_ReturnsContext()
    {
        var handler = new TestHandler();
        var task = new TaskModel { ID = 1, DataTime = DateTime.Now };

        var ctx = handler.Process(task);
        Assert.NotNull(ctx);
        Assert.Equal(1, ctx.Total);
        Assert.Equal(1, ctx.Success);
        Assert.Equal(handler, ctx.Handler);
        Assert.Equal(task, ctx.Task);
    }

    [Fact]
    public void Process_Exception_ReturnsError()
    {
        var handler = new TestHandler();
        // 覆写Execute来模拟异常
        Handler errorHandler = new ErrorHandler();
        var task = new TaskModel { ID = 2, DataTime = DateTime.Now };

        var ctx = errorHandler.Process(task);
        Assert.NotNull(ctx);
        Assert.NotNull(ctx.Error);
        Assert.Contains("测试异常", ctx.Error.Message);
    }

    [Fact(DisplayName = "结束回调异常不会中断执行流程")]
    public void Process_FinishException_ReturnsContext()
    {
        var handler = new FinishThrowsHandler();
        var task = new TaskModel { ID = 3, DataTime = DateTime.Now };

        // 任务已执行完毕，上报失败只能记录，不能把异常抛给线程池
        var ctx = handler.Process(task);

        Assert.NotNull(ctx);
        Assert.Equal(1, ctx.Success);
        Assert.Null(ctx.Error);
    }

    [Fact]
    public void CheckAlive_Disabled_ReturnsTrue()
    {
        var handler = new TestHandler { MaxInactiveTime = 0 };
        Assert.True(handler.CheckAlive());
    }

    [Fact]
    public void CheckAlive_NoTasks_ReturnsTrue()
    {
        var handler = new TestHandler { MaxInactiveTime = 10 };
        Assert.True(handler.CheckAlive());
    }
}

/// <summary>模拟异常的处理器</summary>
internal class ErrorHandler : Handler
{
    public override Int32 Execute(JobContext ctx)
    {
        throw new InvalidOperationException("测试异常");
    }
}
