using System;
using System.Threading;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using NewLife.Caching;
using NewLife.Log;
using Xunit;

namespace AntTest;

/// <summary>获取作业锁失败的场景。锁竞争超时时不能继续分配，否则会切出重复任务</summary>
[Collection("Integration")]
public class AcquireLockTests(RpcServerFixture fixture) : IClassFixture<RpcServerFixture>
{
    [Fact(DisplayName = "作业锁超时返回空任务")]
    public void Acquire_LockTimeout_ReturnsEmpty()
    {
        var name = "SrvLock_" + Guid.NewGuid().ToString("N")[..6];
        fixture.AppService.Login(new LoginModel { Code = name, Secret = "" }, $"{name}@127.0.0.1", "127.0.0.1");

        var app = App.FindByName(name);
        var jobName = "LockJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddHours(-2), Step = 30, Offset = 15 }]);

        var provider = new NoLockProvider();
        var service = new JobService(fixture.AppService, provider, fixture.Provider.GetService<ITracer>(), null);
        var online = fixture.AppService.GetOnline(app, $"{name}@lock", "127.0.0.1");

        var tasks = service.Acquire(app, new AcquireModel { Job = jobName, Count = 1 }, online);

        Assert.Empty(tasks);
        Assert.Equal(1, provider.Calls);
    }

    private class NoLockProvider : CacheProvider
    {
        public Int32 Calls;

        public override IDisposable AcquireLock(String lockKey, Int32 msTimeout)
        {
            Interlocked.Increment(ref Calls);

            // 模拟竞争超时：拿不到锁
            return null;
        }
    }
}
