using System;
using System.Linq;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Providers;
using AntJob.Server;
using NewLife;
using Xunit;

namespace AntTest;

/// <summary>蚂蚁客户端与网络提供者测试（真实 TCP RPC 调度中心）</summary>
[Collection("Integration")]
public class RpcClientTests(RpcServerFixture fixture) : IClassFixture<RpcServerFixture>
{
    private AntSetting CreateSetting(String appId) => new()
    {
        Server = fixture.Server,
        AppId = appId,
        Secret = "",
    };

    [Fact]
    public async Task AntClient_Login_And_GetJobs()
    {
        var client = new AntClient(CreateSetting("RpcClientApp")) { ServiceProvider = fixture.Provider };
        await client.Login();

        var jobs = await client.GetJobs();
        Assert.NotNull(jobs);

        var added = await client.AddJobs([new JobModel { Name = "RpcJob1", Mode = JobModes.Time, DataTime = DateTime.Today }]);
        Assert.Contains("RpcJob1", added);

        jobs = await client.GetJobs();
        Assert.Contains(jobs, e => e.Name == "RpcJob1");

        var peers = await client.GetPeers();
        Assert.NotNull(peers);
        Assert.NotEmpty(peers);

        await client.Logout("test");
        client.Dispose();
    }

    [Fact]
    public async Task AntClient_Acquire_And_Report()
    {
        var client = new AntClient(CreateSetting("RpcAcquireApp")) { ServiceProvider = fixture.Provider };
        await client.Login();

        await client.AddJobs([new JobModel { Name = "RpcAcquireJob", Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddHours(-2), Step = 30, Offset = 15, MaxTask = 4 }]);

        var tasks = await client.Acquire("RpcAcquireJob", null, 2);
        Assert.NotNull(tasks);
        Assert.NotEmpty(tasks);

        var task = tasks[0];
        var rs = await client.Report(new TaskResult { ID = task.ID, Status = JobStatus.完成, Total = 1, Success = 1 });
        Assert.True(rs);

        client.Dispose();
    }

    [Fact]
    public async Task AntClient_Produce_Messages()
    {
        var client = new AntClient(CreateSetting("RpcProduceApp")) { ServiceProvider = fixture.Provider };
        await client.Login();

        await client.AddJobs([new JobModel { Name = "RpcProduceJob", Mode = JobModes.Message, Topic = "T1" }]);

        var count = await client.Produce(new AntJob.Models.ProduceModel
        {
            Job = "RpcProduceJob",
            Topic = "T1",
            Messages = ["a", "b"],
        });

        Assert.True(count >= 1);

        client.Dispose();
    }

    [Fact]
    public async Task NetworkJobProvider_Start_And_GetJobs()
    {
        var setting = CreateSetting("NetProviderApp");
        var scheduler = new Scheduler();
        scheduler.AddHandler(new NetTestHandler());

        var provider = new NetworkJobProvider(setting) { Schedule = scheduler };
        await provider.Start();

        Assert.NotNull(provider.Ant);

        // Start 已把处理器作业注册到调度中心
        var jobs = await provider.GetJobs();
        Assert.NotNull(jobs);
        Assert.Contains(jobs, e => e.Name == "NetTest");

        // 生产消息
        var count = await provider.Produce("NetTest", "T1", ["m1"]);
        Assert.True(count >= 0);

        await provider.Stop();
        Assert.Null(provider.Ant);

        provider.Dispose();
    }

    [Fact]
    public async Task NetworkJobProvider_SetJob()
    {
        var setting = CreateSetting("NetSetJobApp");
        var scheduler = new Scheduler();
        scheduler.AddHandler(new NetTestHandler());

        var provider = new NetworkJobProvider(setting) { Schedule = scheduler };
        await provider.Start();

        // 时间字段按UTC传输，服务端转换回本地时间后应保持一致
        var dataTime = DateTime.Today.AddHours(1);
        var job = new JobModel { Name = "NetTest", Enable = true, DataTime = dataTime };
        var rs = await provider.SetJob(job);
        Assert.NotNull(rs);

        var app = App.FindByName(setting.AppId);
        var entity = Job.FindByAppIDAndName(app.ID, "NetTest");
        Assert.Equal(dataTime, entity.DataTime);

        await provider.Stop();
        provider.Dispose();
    }
}

/// <summary>网络提供者测试用处理器</summary>
internal class NetTestHandler : Handler
{
    public override Int32 Execute(JobContext ctx) => 1;
}

