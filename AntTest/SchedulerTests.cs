using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Handlers;
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
        _scheduler.AddHandler(new TestHandler { Name = "JobA" });
        _scheduler.AddHandler(new TestHandler { Name = "JobB" });
        _scheduler.AddHandler(new TestHandler { Name = "JobC" });

        Assert.Equal(3, _scheduler.Handlers.Count);
    }

    [Fact(DisplayName = "重复添加同类型同名处理器时去重")]
    public void AddHandler_Deduplicates()
    {
        _scheduler.AddHandler(new TestHandler());
        _scheduler.AddHandler(new TestHandler());

        // 启动失败重试会再次解析处理器，不能重复执行同一个作业
        Assert.Single(_scheduler.Handlers);
    }

    [Fact]
    public void AddHandler_T_Registers()
    {
        _scheduler.AddHandler<TestHandler>();
        // DI容器注册不会直接添加到Handlers集合（需要Start时从容器解析）
        Assert.Empty(_scheduler.Handlers);
    }

    [Fact(DisplayName = "连接失败时不保留客户端")]
    public async Task NetworkJobProvider_Start_Failure_Disposes()
    {
        var provider = new NetworkJobProvider(new AntSetting { Server = "http://127.0.0.1:1", AppId = "FailApp" });

        await Assert.ThrowsAnyAsync<Exception>(() => provider.Start());

        // 登录失败时客户端已被释放，定时重连不会累积连接
        Assert.Null(provider.Ant);

        provider.Dispose();
    }

    [Fact(DisplayName = "停止调度器时并行停止全部处理器")]
    public async Task Stop_Stops_All_Handlers()
    {
        var provider = new CountingProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        var h1 = new TestHandler { Name = "MsgJob" };
        var h2 = new TestHandler { Name = "OtherJob" };
        _scheduler.AddHandler(h1);
        _scheduler.AddHandler(h2);

        // 处理器必须处于工作状态才能被停止
        await _scheduler.Process();
        Assert.True(h1.Active);
        Assert.True(h2.Active);

        _scheduler.Stop();

        Assert.False(h1.Active);
        Assert.False(h2.Active);
    }

    [Fact(DisplayName = "调度异常后仍可继续下一轮")]
    public async Task Process_Recovers_After_Exception()
    {
        var provider = new ThrowingProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        _scheduler.AddHandler(new TestHandler { Name = "ErrJob", Mode = JobModes.Time });

        // 取作业失败时异常向上抛出，交由定时器记录
        await Assert.ThrowsAsync<InvalidOperationException>(() => _scheduler.Process());

        // 单飞标记必须被释放，否则后续轮次永远被跳过
        provider.Throw = false;
        Assert.False(await _scheduler.Process());
    }

    [Fact(DisplayName = "停用作业时停止处理器不阻塞调度轮次")]
    public async Task Process_Disabled_Job_Does_Not_Block()
    {
        var provider = new BlockingTaskProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        var handler = new BlockingHandler { Name = "DisableJob", Mode = JobModes.Time, Release = provider.Release };
        _scheduler.AddHandler(handler);

        // 首轮把任务交给处理器，Execute 阻塞不退，Busy 保持为1
        await _scheduler.Process();
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (handler.Busy == 0 && DateTime.UtcNow < deadline) await Task.Delay(50);
        Assert.True(handler.Busy > 0);

        // 服务端停用作业
        provider.Enable = false;
        var sw = Stopwatch.StartNew();
        await _scheduler.Process();
        sw.Stop();

        // 停止需要等待在途任务，必须转到后台，本轮调度立即返回
        Assert.True(sw.ElapsedMilliseconds < 2_000, $"调度轮次被阻塞 {sw.ElapsedMilliseconds}ms");
        Assert.True(handler.Stopping || !handler.Active);

        // 在途任务结束后，后台停止完成
        provider.Release.Set();
        deadline = DateTime.UtcNow.AddSeconds(10);
        while (handler.Stopping && DateTime.UtcNow < deadline) await Task.Delay(50);

        Assert.False(handler.Stopping);
        Assert.False(handler.Active);
    }

    [Fact(DisplayName = "服务端作业可通过类名动态创建处理器")]
    public async Task Process_Creates_Handler_By_ClassName()
    {
        var provider = new DynamicHandlerProvider { ClassName = typeof(DynamicHandler).FullName };
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        // 本地不注册任何处理器，只能靠服务端下发的类名创建
        Assert.Empty(_scheduler.Handlers);

        await _scheduler.Process();

        Assert.Single(_scheduler.Handlers);
        Assert.IsType<DynamicHandler>(_scheduler.Handlers[0]);
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

    [Fact(DisplayName = "多个作业并行申请任务")]
    public async Task Process_Acquires_InParallel()
    {
        var provider = new ParallelAcquireProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        for (var i = 0; i < 3; i++)
        {
            _scheduler.AddHandler(new TestHandler { Name = $"Job{i}", Mode = JobModes.Time });
        }

        var rs = await _scheduler.Process();

        Assert.Equal(3, provider.AcquireCount);
        Assert.All(provider.Waited, Assert.True);
        Assert.False(rs);
    }

    [Fact(DisplayName = "上一轮未结束时跳过新的调度轮次")]
    public async Task Process_Skips_When_Running()
    {
        var provider = new BlockingProvider();
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        _scheduler.AddHandler(new TestHandler { Name = "BlockJob", Mode = JobModes.Time });

        var first = _scheduler.Process();
        await provider.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // 首轮阻塞在 GetJobs 时，定时器再次触发不应重复申请
        Assert.False(await _scheduler.Process().WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.Equal(1, provider.GetJobsCount);

        provider.Release.TrySetResult();

        // 跳过的那一轮会被补偿，首轮返回true提示立即再来一轮
        Assert.True(await first);

        await _scheduler.Process();
        Assert.Equal(2, provider.GetJobsCount);
    }

    [Fact]
    public async Task Process_Syncs_JobTopic_To_MessageHandler()
    {
        var provider = new CountingProvider { JobTopic = "T2" };
        _scheduler.Provider = provider;
        provider.Schedule = _scheduler;

        var handler = new TestMessageHandler { Name = "MsgJob", Topic = "T1" };
        _scheduler.AddHandler(handler);

        await _scheduler.Process();

        Assert.Equal("T2", handler.Topic);
    }

    private class TestMessageHandler : MessageHandler
    {
        public override Int32 Execute(JobContext ctx) => 1;
    }

    private class ParallelAcquireProvider : NetworkJobProvider
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ParallelAcquireProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public Int32 AcquireCount;

        public List<Boolean> Waited { get; } = [];

        public override Task<IJob[]> GetJobs() => Task.FromResult<IJob[]>([
            new JobModel { Name = "Job0", Mode = JobModes.Time, Enable = true, MaxTask = 1 },
            new JobModel { Name = "Job1", Mode = JobModes.Time, Enable = true, MaxTask = 1 },
            new JobModel { Name = "Job2", Mode = JobModes.Time, Enable = true, MaxTask = 1 },
        ]);

        public override async Task<ITask[]> Acquire(IJob job, String topic, Int32 count)
        {
            // 等到三个申请都进入，才能证明它们是并行发起的；串行申请会在此超时
            if (Interlocked.Increment(ref AcquireCount) >= 3) _release.TrySetResult();

            var ok = await Task.WhenAny(_release.Task, Task.Delay(2_000)) == _release.Task;
            lock (Waited) Waited.Add(ok);

            return [];
        }
    }

    private class DynamicHandlerProvider : NetworkJobProvider
    {
        public DynamicHandlerProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public String ClassName { get; set; }

        public override Task<IJob[]> GetJobs() => Task.FromResult<IJob[]>(
            [new JobModel { Name = "DynamicJob", Mode = JobModes.Time, Enable = true, ClassName = ClassName, MaxTask = 1 }]);

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count) => Task.FromResult(Array.Empty<ITask>());
    }

    private class BlockingTaskProvider : NetworkJobProvider
    {
        public BlockingTaskProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        /// <summary>是否启用作业。修改后立即生效，用于模拟服务端停用</summary>
        public Boolean Enable { get; set; } = true;

        /// <summary>放行阻塞中的任务</summary>
        public ManualResetEventSlim Release { get; } = new(false);

        public override Task<IJob[]> GetJobs() => Task.FromResult<IJob[]>(
            [new JobModel { Name = "DisableJob", Mode = JobModes.Time, Enable = Enable, MaxTask = 1 }]);

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count)
        {
            if (!Enable) return Task.FromResult(Array.Empty<ITask>());

            return Task.FromResult<ITask[]>([new TaskModel { ID = 991, DataTime = DateTime.Now }]);
        }
    }

    private class BlockingHandler : Handler
    {
        public ManualResetEventSlim Release { get; set; } = new(false);

        public override Int32 Execute(JobContext ctx)
        {
            // 模拟长时间不退出的任务，最长等待10秒
            Release.Wait(10_000);

            return 1;
        }
    }

    /// <summary>由服务端类名动态创建的处理器</summary>
    private class DynamicHandler : Handler
    {
        public override Int32 Execute(JobContext ctx) => 1;
    }

    private class ThrowingProvider : NetworkJobProvider
    {
        public ThrowingProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public Boolean Throw { get; set; } = true;

        public override Task<IJob[]> GetJobs()
        {
            if (Throw) throw new InvalidOperationException("模拟调度中心不可用");

            return Task.FromResult<IJob[]>([new JobModel { Name = "ErrJob", Mode = JobModes.Time, Enable = true, MaxTask = 1 }]);
        }

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count) => Task.FromResult(Array.Empty<ITask>());
    }

    private class BlockingProvider : NetworkJobProvider
    {
        public BlockingProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Int32 GetJobsCount;

        public override async Task<IJob[]> GetJobs()
        {
            Interlocked.Increment(ref GetJobsCount);
            Entered.TrySetResult();

            await Release.Task;

            return [new JobModel { Name = "BlockJob", Mode = JobModes.Time, Enable = true, MaxTask = 1 }];
        }

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count) => Task.FromResult(Array.Empty<ITask>());
    }

    [Fact(DisplayName = "未注入服务提供者时调度器仍可启动")]
    public void Start_WithoutServiceProvider_DoesNotThrow()
    {
        // 手工实例化调度器（不经过依赖注入）同样受支持，例如控制台示例
        using var scheduler = new Scheduler { Provider = new TestJobProvider() };
        scheduler.AddHandler(new AgentTestHandler());

#pragma warning disable CS0618 // Start 已过时，这里需要同步等待启动流程完成
        scheduler.Start();
#pragma warning restore CS0618

        Assert.NotNull(scheduler.Provider);
        Assert.False(scheduler.Handlers[0].Active);
    }

    private class CountingProvider : NetworkJobProvider
    {
        public CountingProvider() : base(new AntSetting { Server = "http://127.0.0.1:1" }) { }

        public override Int32 MessageIdlePeriod => 15;

        public Int32 AcquireCount;

        public String JobTopic { get; set; } = "T1";

        public override Task<IJob[]> GetJobs() => Task.FromResult<IJob[]>([
            new JobModel { Name = "MsgJob", Mode = JobModes.Message, Enable = true, MaxTask = 1, Topic = JobTopic },
            new JobModel { Name = "OtherJob", Mode = JobModes.Message, Enable = true, MaxTask = 1, Topic = JobTopic },
        ]);

        public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count)
        {
            AcquireCount++;

            return Task.FromResult(Array.Empty<ITask>());
        }
    }
}
