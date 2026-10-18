using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server;
using AntJob.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife;
using NewLife.Log;
using NewLife.Model;
using NewLife.Remoting.Models;
using Xunit;

namespace AntTest;

/// <summary>调度中心服务测试（应用认证/作业调度/后台服务/配置）</summary>
[Collection("Integration")]
public class ServerTests(RpcServerFixture fixture) : IClassFixture<RpcServerFixture>
{
    private LoginResponse Login(AppService appService, String name)
    {
        var (app, _, rs) = appService.Login(new LoginModel { Code = name, Secret = "" }, $"{name}@127.0.0.1", "127.0.0.1");
        Assert.NotNull(app);
        return rs;
    }

    [Fact]
    public void AppService_Login_AutoRegistry()
    {
        var name = "SrvApp_" + Guid.NewGuid().ToString("N")[..6];
        var rs = Login(fixture.AppService, name);

        var app = App.FindByName(name);
        Assert.NotNull(app);
        Assert.True(app.ID > 0);
        Assert.True(app.Enable);

        Assert.Equal(name, rs.Name);
    }

    [Fact]
    public void AppService_GetOnline_And_History()
    {
        var name = "SrvOnline_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, "session1", "127.0.0.1");
        Assert.NotNull(online);
        Assert.Equal(app.ID, online.AppID);

        fixture.AppService.WriteHistory(app, "UnitTest", true, "remark", "127.0.0.1");

        // 历史异步落库，轮询等待
        var found = WaitUntil(() => AppHistory.FindAllByAppIDAndAction(app.ID, "UnitTest").Any());
        Assert.True(found);
    }

    [Fact]
    public void AppService_IssueToken_And_Decode()
    {
        var name = "SrvToken_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var token = fixture.AppService.IssueToken(name, fixture.Setting);
        Assert.NotNull(token);
        Assert.False(token.AccessToken.IsNullOrEmpty());

        var (app, ex) = fixture.AppService.DecodeToken(token.AccessToken, fixture.Setting.TokenSecret);
        Assert.Null(ex);
        Assert.NotNull(app);
        Assert.Equal(name, app.Name);
    }

    [Fact]
    public void JobService_AddJobs_And_GetJobs()
    {
        var name = "SrvJob_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        var rs = fixture.JobService.AddJobs(app, [new JobModel
        {
            Name = "SrvJob1",
            Enable = true,
            Mode = JobModes.Time,
            DataTime = DateTime.Today,
            Step = 30,
            Offset = 15,
            DisplayName = "服务测试作业",
        }]);
        Assert.Contains("SrvJob1", rs);

        var jobs = fixture.JobService.GetJobs(app);
        Assert.Contains(jobs, e => e.Name == "SrvJob1");
    }

    [Fact]
    public async Task JobService_Produce_And_Acquire_Message()
    {
        var name = "SrvMsg_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        fixture.JobService.AddJobs(app, [new JobModel { Name = "SrvMsgJob", Enable = true, Mode = JobModes.Message, Topic = "T1" }]);

        var count = fixture.JobService.Produce(app, new ProduceModel
        {
            Job = "SrvMsgJob",
            Topic = "T1",
            Messages = ["a", "b"],
        });
        Assert.Equal(2, count);

        // 消息数异步回写，轮询等待
        Assert.True(WaitUntil(() => Job.FindByAppIDAndName(app.ID, "SrvMsgJob")?.MessageCount >= 2));

        var online = fixture.AppService.GetOnline(app, "session2", "127.0.0.1");
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = "SrvMsgJob", Topic = "T1", Count = 2 }, online);
        Assert.Single(tasks);
        Assert.Contains("a", tasks[0].Data);
        Assert.Contains("b", tasks[0].Data);

        var rs = fixture.JobService.Report(app, new TaskResult { ID = tasks[0].ID, Status = JobStatus.完成, Total = 1, Success = 1 }, online);
        Assert.True(rs);

        await Task.CompletedTask;
    }

    [Fact]
    public void JobService_SetJob_UpdatesFields()
    {
        var name = "SrvSet_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        fixture.JobService.AddJobs(app, [new JobModel { Name = "SrvSetJob", Enable = true, Mode = JobModes.Time, DataTime = DateTime.Today, Step = 30 }]);

        var job = fixture.JobService.SetJob(app, new JobModel { Name = "SrvSetJob", Step = 60, DataTime = DateTime.Today.AddHours(1) },
            new System.Collections.Generic.Dictionary<String, Object> { ["Step"] = null, ["DataTime"] = null });
        Assert.NotNull(job);
        Assert.Equal(60, job.Step);
    }

    [Fact]
    public void Job_Insert_Notify_Only_When_Enabled()
    {
        var name = "SrvNotify_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        var count = 0;
        EventHandler<JobChangedEventArgs> handler = (s, e) => count++;
        Job.Changed += handler;
        try
        {
            new Job { AppID = app.ID, Name = "DisabledJob", Enable = false, Mode = JobModes.Time }.Insert();
            Assert.Equal(0, count);

            new Job { AppID = app.ID, Name = "EnabledJob", Enable = true, Mode = JobModes.Time }.Insert();
            Assert.Equal(1, count);
        }
        finally
        {
            Job.Changed -= handler;
        }
    }

    [Fact]
    public void JobService_SetJob_Utc_ConvertsToLocal()
    {
        var name = "SrvUtc_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        fixture.JobService.AddJobs(app, [new JobModel { Name = "SrvUtcJob", Enable = true, Mode = JobModes.Time, DataTime = DateTime.Today, Step = 30 }]);

        var utc = new DateTime(2026, 10, 18, 6, 0, 0, DateTimeKind.Utc);
        var job = fixture.JobService.SetJob(app, new JobModel { Name = "SrvUtcJob", DataTime = utc },
            new System.Collections.Generic.Dictionary<String, Object> { ["DataTime"] = null, ["__utc"] = null });

        Assert.NotNull(job);
        var entity = Job.FindByAppIDAndName(app.ID, "SrvUtcJob");
        Assert.Equal(utc.ToLocalTime(), entity.DataTime);
    }

    [Fact]
    public void JobService_SetJob_WithoutUtc_KeepsLocal()
    {
        var name = "SrvLocal_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);
        var app = App.FindByName(name);

        fixture.JobService.AddJobs(app, [new JobModel { Name = "SrvLocalJob", Enable = true, Mode = JobModes.Time, DataTime = DateTime.Today, Step = 30 }]);

        // 旧版客户端不带 __utc，服务端按本地时间处理
        var local = new DateTime(2026, 10, 18, 14, 0, 0, DateTimeKind.Local);
        var job = fixture.JobService.SetJob(app, new JobModel { Name = "SrvLocalJob", DataTime = local },
            new System.Collections.Generic.Dictionary<String, Object> { ["DataTime"] = null });

        Assert.NotNull(job);
        var entity = Job.FindByAppIDAndName(app.ID, "SrvLocalJob");
        Assert.Equal(local, entity.DataTime);
    }

    [Fact]
    public async Task Worker_Start_And_Stop()
    {
        var setting = new AntJobSetting { Port = 0, AutoRegistry = true, TokenSecret = "HS256:antjob_test_secret" };
        var worker = new Worker(fixture.CacheProvider, BuildProvider(fixture), setting, DefaultTracer.Instance ?? new DefaultTracer());

        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void AntJobSetting_Defaults()
    {
        var setting = new AntJobSetting();
        Assert.Equal(9999, setting.Port);
        Assert.True(setting.Debug);
        Assert.True(setting.AutoRegistry);
        Assert.Equal(2 * 3600, setting.TokenExpire);
        Assert.Equal(600, setting.SessionTimeout);
    }

    [Fact]
    public async Task AntJobWorker_Starts_Scheduler()
    {
        var set = AntSetting.Current;
        var oldServer = set.Server;
        var oldAppId = set.AppId;
        try
        {
            set.Server = fixture.Server;
            set.AppId = "WorkerApp_" + Guid.NewGuid().ToString("N")[..6];
            set.Secret = "";

            var container = new ObjectContainer();
            container.AddAntJob();

            var host = container.BuildHost();
            await host.StartAsync(CancellationToken.None);

            var scheduler = host.Services.GetService(typeof(Scheduler)) as Scheduler;
            Assert.NotNull(scheduler);
            Assert.NotNull(scheduler.Setting);
            Assert.Equal(set.Server, scheduler.Setting.Server);

            await host.StopAsync(CancellationToken.None);
            (host as IDisposable)?.Dispose();
        }
        finally
        {
            set.Server = oldServer;
            set.AppId = oldAppId;
        }
    }

    private static Boolean WaitUntil(Func<Boolean> func, Int32 timeout = 15000)
    {
        var end = DateTime.Now.AddMilliseconds(timeout);
        while (DateTime.Now < end)
        {
            if (func()) return true;
            Task.Delay(50).Wait();
        }

        return func();
    }

    private static IServiceProvider BuildProvider(RpcServerFixture fixture)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddSingleton(fixture.Setting);
        services.AddSingleton(fixture.AppService);
        services.AddSingleton(fixture.JobService);
        services.AddSingleton(fixture.CacheProvider);
        return services.BuildServiceProvider();
    }
}
