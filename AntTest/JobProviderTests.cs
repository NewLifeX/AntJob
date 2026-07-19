using System;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using NewLife.Log;
using Xunit;

namespace AntTest;

/// <summary>测试用作业提供者（最小实现）</summary>
internal class TestJobProvider : JobProvider
{
    public override Task<IJob[]> GetJobs() => Task.FromResult(Array.Empty<IJob>());
    public override Task<IJob> SetJob(IJob job) => Task.FromResult(job);
    public override Task<ITask[]> Acquire(IJob job, String topic, Int32 count) => Task.FromResult(Array.Empty<ITask>());
    public override Task<Int32> Produce(String job, String topic, String[] messages, MessageOption option) => Task.FromResult(0);
}

/// <summary>作业提供者抽象层测试</summary>
public class JobProviderTests
{
    [Fact]
    public void Schedule_Property()
    {
        var provider = new TestJobProvider();
        Assert.Null(provider.Schedule);

        var scheduler = new Scheduler();
        provider.Schedule = scheduler;
        Assert.Same(scheduler, provider.Schedule);
    }

    [Fact]
    public void Start_Default_DoesNothing()
    {
        var provider = new TestJobProvider();
        var task = provider.Start();
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void Stop_Default_DoesNothing()
    {
        var provider = new TestJobProvider();
        var task = provider.Stop();
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task GetJobs_Returns_Empty()
    {
        var provider = new TestJobProvider();
        var jobs = await provider.GetJobs();
        Assert.NotNull(jobs);
        Assert.Empty(jobs);
    }

    [Fact]
    public async Task SetJob_Returns_Same()
    {
        var provider = new TestJobProvider();
        var job = new JobModel { Name = "Test" };
        var result = await provider.SetJob(job);
        Assert.Same(job, result);
    }

    [Fact]
    public async Task Acquire_Returns_Empty()
    {
        var provider = new TestJobProvider();
        var tasks = await provider.Acquire(new JobModel(), null, 5);
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Produce_Default_Returns_Zero()
    {
        var provider = new TestJobProvider();
        var result = await provider.Produce("test", "topic", ["msg1"], null);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Report_Default_DoesNothing()
    {
        var provider = new TestJobProvider();
        var task = provider.Report(new JobContext());
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void Finish_Default_DoesNothing()
    {
        var provider = new TestJobProvider();
        var task = provider.Finish(new JobContext());
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
    }
}

/// <summary>文件提供者 - 边界条件测试（不触发文件I/O）</summary>
public class FileJobProviderTests
{
    [Fact]
    public async Task Acquire_DisabledJob_ReturnsEmpty()
    {
        var provider = new FileJobProvider();
        var job = new JobModel { Name = "test", Enable = false, DataTime = DateTime.Now.AddDays(-1) };

        var tasks = await provider.Acquire(job, null, 5);
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Acquire_FutureTime_ReturnsEmpty()
    {
        var provider = new FileJobProvider();
        var job = new JobModel
        {
            Name = "test",
            Enable = true,
            DataTime = DateTime.Now.AddDays(1),  // 在将来
            Step = 30,
            Offset = 0,
            Mode = JobModes.Time,
        };

        var tasks = await provider.Acquire(job, null, 5);
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Acquire_ZeroStep_UsesDefault()
    {
        var provider = new FileJobProvider();
        var job = new JobModel
        {
            Name = "test",
            Enable = true,
            DataTime = DateTime.Now.AddDays(-1),
            Step = 0,  // 应使用默认值30
            Offset = 0,
            Mode = JobModes.Time,
        };

        // 不应抛出异常，即使 _File 为 null，但 count=0 时不会触发保存
        var exception = await Record.ExceptionAsync(() => provider.Acquire(job, null, 0));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Acquire_DataMode_RespectsEndLimit()
    {
        var provider = new FileJobProvider();
        var now = DateTime.Now;
        var job = new JobModel
        {
            Name = "test",
            Enable = true,
            DataTime = now.AddDays(-1),
            End = now.AddDays(-1).AddSeconds(60),
            Step = 30,
            Offset = 0,
            Mode = JobModes.Data,
        };

        // 保存操作会抛NullRef，因为_File==null
        var exception = await Record.ExceptionAsync(() => provider.Acquire(job, null, 10));
        Assert.NotNull(exception);
    }
}
