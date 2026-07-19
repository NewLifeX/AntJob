using System;
using System.Linq;
using AntJob;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using Xunit;

namespace AntTest;

/// <summary>调度器基本操作测试</summary>
public class SchedulerTests : IDisposable
{
    private readonly Scheduler _scheduler;

    public SchedulerTests()
    {
        _scheduler = new Scheduler();
    }

    public void Dispose()
    {
        _scheduler.TryDispose();
    }

    [Fact]
    public void Constructor_Handlers_Empty()
    {
        Assert.NotNull(_scheduler.Handlers);
        Assert.Empty(_scheduler.Handlers);
    }

    [Fact]
    public void Constructor_Provider_Null()
    {
        Assert.Null(_scheduler.Provider);
    }

    [Fact]
    public void AddHandler_AddsHandler()
    {
        var handler = new TestHandler();
        _scheduler.AddHandler(handler);

        Assert.Single(_scheduler.Handlers);
        Assert.Same(handler, _scheduler.Handlers[0]);
    }

    [Fact]
    public void AddHandler_Returns_Scheduler()
    {
        var result = _scheduler.AddHandler(new TestHandler());
        Assert.Same(_scheduler, result);
    }

    [Fact]
    public void AddHandler_Multiple()
    {
        _scheduler.AddHandler(new TestHandler());
        _scheduler.AddHandler(new TestHandler());
        _scheduler.AddHandler(new TestHandler());

        Assert.Equal(3, _scheduler.Handlers.Count);
    }

    [Fact]
    public void AddHandler_T_Registers()
    {
        _scheduler.AddHandler<TestHandler>();
        // DI容器注册不会直接添加到Handlers集合（需要Start时从容器解析）
        Assert.Empty(_scheduler.Handlers);
    }

    [Fact]
    public void Setting_Assign()
    {
        var setting = new AntSetting { Server = "tcp://test:9999", AppId = "TestApp" };
        _scheduler.Setting = setting;

        Assert.NotNull(_scheduler.Setting);
        Assert.Equal("tcp://test:9999", _scheduler.Setting.Server);
        Assert.Equal("TestApp", _scheduler.Setting.AppId);
    }

    [Fact]
    public void Provider_Assign()
    {
        var provider = new FileJobProvider();
        _scheduler.Provider = provider;

        Assert.NotNull(_scheduler.Provider);
        Assert.Same(provider, _scheduler.Provider);
    }
}
