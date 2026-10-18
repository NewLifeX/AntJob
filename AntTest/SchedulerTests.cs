using System;
using System.Linq;
using System.Threading.Tasks;
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
    public async Task JoinAsync_Selects_Http_Provider()
    {
        var set = new AntSetting { Server = "http://127.0.0.1:1", AppId = "TestApp" };

        await _scheduler.JoinAsync(set);

        Assert.IsType<HttpJobProvider>(_scheduler.Provider);
    }

    [Fact]
    public async Task JoinAsync_Selects_Rpc_Provider()
    {
        var set = new AntSetting { Server = "tcp://127.0.0.1:1", AppId = "TestApp" };

        await _scheduler.JoinAsync(set);

        Assert.IsType<NetworkJobProvider>(_scheduler.Provider);
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

    [Fact]
    public async Task Process_MessageJob_IdleBackoff()
    {
        var provider = new CountingProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        var handler = new TestHandler { Name = "MsgJob", Mode = JobModes.Message };
        _scheduler.AddHandler(handler);

        await _scheduler.Process();
        Assert.Equal(1, provider.AcquireCount);
        Assert.True(handler.NextAcquire > DateTime.Now);

        // 未到下一次申请时间，直接跳过
        await _scheduler.Process();
        Assert.Equal(1, provider.AcquireCount);

        // 通知到达后重置，立即再次申请
        handler.NextAcquire = DateTime.MinValue;
        await _scheduler.Process();
        Assert.Equal(2, provider.AcquireCount);
    }

    private class CountingProvider : NetworkJobProvider
    {
        public CountingProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public override Int32 MessageIdlePeriod => 15;

        public Int32 AcquireCount;

        public override Task<IJob[]> GetJobs() => Task.FromResult<IJob[]>([new JobModel
        {
            Name = "MsgJob",
            Mode = JobModes.Message,
            Enable = true,
            MaxTask = 1,
            Topic = "T1",
        }]);

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count)
        {
            AcquireCount++;

            return Task.FromResult(Array.Empty<ITask>());
        }
    }
}
