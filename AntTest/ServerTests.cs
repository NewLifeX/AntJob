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

    [Fact(DisplayName = "清理长时间未心跳的在线记录")]
    public void AppService_ClearOnline()
    {
        var name = "SrvClear_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, $"{name}@stale", "127.0.0.1");
        online.Save();
        Assert.True(online.ID > 0);

        // 模拟客户端异常退出后未注销：直接把更新时间改到超时之前
        AppOnline.Meta.Session.Execute($"Update {AppOnline.Meta.TableName} Set UpdateTime='{DateTime.Now.AddHours(-1):yyyy-MM-dd HH:mm:ss}' Where ID={online.ID}");

        var count = AppService.ClearOnline(600);
        Assert.True(count >= 1);
        Assert.Null(AppOnline.Find(AppOnline._.ID == online.ID));
    }

    [Fact(DisplayName = "并发登录只产生一条在线记录")]
    public async Task AppService_Login_Concurrent()
    {
        var name = "SrvLoginRace_" + Guid.NewGuid().ToString("N")[..6];
        var sessionId = $"{name}@127.0.0.1";

        using var barrier = new Barrier(3);
        var tasks = Enumerable.Range(0, 3).Select(_ => Task.Run(() =>
        {
            // 同时进入登录，覆盖并发写入同一在线实例的场景
            barrier.SignalAndWait();
            return fixture.AppService.Login(new LoginModel { Code = name, Secret = "" }, sessionId, "127.0.0.1");
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.All(tasks, t => Assert.NotNull(t.Result.Item2));
        Assert.Single(AppOnline.FindAll(AppOnline._.Instance == sessionId));
    }

    [Fact(DisplayName = "并发上报累计在线统计")]
    public async Task AppService_UpdateOnline_Concurrent()
    {
        var name = "SrvOnlineSum_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, $"{name}@sum", "127.0.0.1");
        online.Save();

        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
            fixture.AppService.UpdateOnline(app, new JobTask { Total = 1, Success = 1, Cost = 1, Speed = 1, Status = JobStatus.完成 }, online))).ToArray();
        await Task.WhenAll(tasks);

        var entity = AppOnline.Find(AppOnline._.ID == online.ID);
        Assert.NotNull(entity);
        Assert.Equal(10, entity.Total);
        Assert.Equal(10, entity.Success);
    }

    [Fact(DisplayName = "心跳刷新在线记录更新时间")]
    public void AppService_ClearOnline_KeepsRefreshed()
    {
        var name = "SrvClearKeep_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, $"{name}@keep", "127.0.0.1");
        online.Save();

        // 模拟长时间未心跳：数据库与内存实体都退回到一小时前
        var old = DateTime.Now.AddHours(-1);
        AppOnline.Meta.Session.Execute($"Update {AppOnline.Meta.TableName} Set UpdateTime='{old:yyyy-MM-dd HH:mm:ss}' Where ID={online.ID}");
        var fresh = AppOnline.FindByID(online.ID);
        Assert.True(fresh.UpdateTime < DateTime.Now.AddMinutes(-30));

        // 心跳必须把更新时间推进，否则在线记录会被周期清理误删
        var rs = fixture.AppService.Ping(app, fresh, new PingRequest(), "10.0.0.9");
        Assert.NotNull(rs);

        var entity = AppOnline.Find(AppOnline._.ID == online.ID);
        Assert.NotNull(entity);
        Assert.True(entity.UpdateTime > DateTime.Now.AddMinutes(-1), $"心跳未刷新更新时间：{entity.UpdateTime:O}");

        // 条件删除不能误删刚刷新过的记录
        AppService.ClearOnline(600);
        Assert.NotNull(AppOnline.Find(AppOnline._.ID == online.ID));
    }

    [Fact(DisplayName = "生产消息返回时消息数已落库")]
    public void JobService_Produce_CountPersisted()
    {
        var name = "SrvProduceCnt_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "ProduceCntJob";
        var topic = "T1";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Message, Topic = topic, Enable = true }]);

        var job = Job.FindByAppIDAndName(app.ID, jobName);
        Assert.NotNull(job);

        // 消费者刚申请过且没有消息，2分钟内的兜底查询会被跳过，因此计数必须立即可见
        fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Topic = topic, Count = 1 }, fixture.AppService.GetOnline(app, $"{name}@cons", "127.0.0.1"));

        var count = fixture.JobService.Produce(app, new ProduceModel { Job = jobName, Topic = topic, Messages = ["m1", "m2"] });
        Assert.Equal(2, count);

        var job2 = Job.FindByAppIDAndName(app.ID, jobName);
        Assert.Equal(2, job2.MessageCount);

        // 计数立即可见，随后的申请必须拿到任务
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Topic = topic, Count = 1 }, fixture.AppService.GetOnline(app, $"{name}@cons", "127.0.0.1"));
        Assert.NotEmpty(tasks);
    }

    [Fact(DisplayName = "忽略已完成任务的迟到上报")]
    public void JobService_Report_IgnoresLateReport()
    {
        var name = "SrvLateReport_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "LateReportJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddHours(-2), Step = 30, Offset = 15 }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@late", "127.0.0.1");
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Count = 1 }, online);
        Assert.NotEmpty(tasks);

        var id = tasks[0].ID;
        var done = fixture.JobService.Report(app, new TaskResult { ID = id, Status = JobStatus.完成, Total = 5, Success = 5 }, online);
        Assert.True(done);

        var job1 = Job.FindByAppIDAndName(app.ID, jobName);
        var dataTime1 = job1.DataTime;

        // 迟到的中间状态上报不能把已完成任务回退，也不能重复推进作业
        var late = fixture.JobService.Report(app, new TaskResult { ID = id, Status = JobStatus.处理中, Total = 1, Success = 0 }, online);
        Assert.True(late);

        var task = JobTask.FindByID(id);
        Assert.Equal(JobStatus.完成, task.Status);
        Assert.Equal(5, task.Success);

        var job2 = Job.FindByAppIDAndName(app.ID, jobName);
        Assert.Equal(dataTime1, job2.DataTime);
    }

    [Fact(DisplayName = "并发申请累计在线任务数")]
    public async Task JobService_Acquire_Concurrent_Tasks()
    {
        var name = "SrvAcqSum_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "AcqSumJob";
        // 足够长的历史区间，保证并发申请都能切到分片
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddDays(-5), Step = 30, Offset = 15 }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@acq", "127.0.0.1");
        online.Save();

        var tasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Count = 2 }, online))).ToArray();
        var results = await Task.WhenAll(tasks);

        var total = results.Sum(e => e.Length);

        var entity = AppOnline.Find(AppOnline._.ID == online.ID);
        Assert.NotNull(entity);
        Assert.Equal(total, entity.Tasks);
    }

    [Fact(DisplayName = "并发上报累计作业统计")]
    public async Task JobService_Report_Concurrent_Stats()
    {
        var name = "SrvRepSum_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "RepSumJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddDays(-5), Step = 30, Offset = 15 }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@rep", "127.0.0.1");
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Count = 10 }, online);
        Assert.Equal(10, tasks.Length);

        var before = Job.FindByAppIDAndName(app.ID, jobName);

        var reports = tasks.Select(e => Task.Run(() => fixture.JobService.Report(app, new TaskResult
        {
            ID = e.ID,
            Status = JobStatus.完成,
            Total = 1,
            Success = 1,
            Cost = 10,
        }, online))).ToArray();
        await Task.WhenAll(reports);

        var job = Job.FindByAppIDAndName(app.ID, jobName);
        Assert.Equal(before.Total + 10, job.Total);
        Assert.Equal(before.Success + 10, job.Success);
        Assert.Equal(before.Times + 10, job.Times);
    }

    [Fact(DisplayName = "消息消费后去重缓存被清理")]
    public void JobService_Produce_UniqueAfterConsume()
    {
        var name = "SrvUnique_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "UniqueJob";
        var topic = "U1";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Message, Topic = topic, Enable = true }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@uniq", "127.0.0.1");
        var model = new ProduceModel { Job = jobName, Topic = topic, Messages = ["u1"], Unique = true };

        Assert.Equal(1, fixture.JobService.Produce(app, model));

        // 未消费前重复生产应被去重
        Assert.Equal(0, fixture.JobService.Produce(app, model));

        // 消费掉消息
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Topic = topic, Count = 1 }, online);
        Assert.NotEmpty(tasks);

        // 消费完成后同一条消息应可再次生产
        Assert.Equal(1, fixture.JobService.Produce(app, model));
    }

    [Fact(DisplayName = "上报记录客户端链路标识")]
    public void JobService_Report_StoresTraceId()
    {
        var name = "SrvTrace_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "TraceJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddHours(-2), Step = 30, Offset = 15 }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@trace", "127.0.0.1");
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Count = 1 }, online);
        Assert.NotEmpty(tasks);

        var traceId = "trace-" + Guid.NewGuid().ToString("N")[..8];
        fixture.JobService.Report(app, new TaskResult { ID = tasks[0].ID, Status = JobStatus.完成, Total = 1, Success = 1, TraceId = traceId }, online);

        var task = JobTask.FindByID(tasks[0].ID);
        Assert.Equal(traceId, task.TraceId);
    }

    [Fact(DisplayName = "注册作业后刷新应用作业数")]
    public void JobService_AddJobs_Refreshes_JobCount()
    {
        var name = "SrvJobCnt_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        app.JobCount = 0;
        app.Save();

        var jobName = "JobCntJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today }]);

        var entity = App.FindByName(name);
        Assert.True(entity.JobCount >= 1);
        Assert.Equal(Job.FindCountByAppID(entity.ID), entity.JobCount);
    }

    [Fact(DisplayName = "按会话超时配置清理在线记录")]
    public void AppService_ClearOnline_Uses_Timeout()
    {
        var name = "SrvClearCfg_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, $"{name}@cfg", "127.0.0.1");
        online.Save();

        // 记录停在5分钟前：600秒超时下应保留，60秒超时下应清理
        var old = DateTime.Now.AddMinutes(-5);
        AppOnline.Meta.Session.Execute($"Update {AppOnline.Meta.TableName} Set UpdateTime='{old:yyyy-MM-dd HH:mm:ss}' Where ID={online.ID}");

        AppService.ClearOnline(600);
        Assert.NotNull(AppOnline.Find(AppOnline._.ID == online.ID));

        AppService.ClearOnline(60);
        Assert.Null(AppOnline.Find(AppOnline._.ID == online.ID));
    }

    [Fact(DisplayName = "并发生产累计应用消息数")]
    public async Task JobService_Produce_Concurrent_MessageCount()
    {
        var name = "SrvProdSum_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        app.MessageCount = 0;
        app.Save();

        var jobName = "ProdSumJob";
        var topic = "P1";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Message, Topic = topic, Enable = true }]);

        var before = App.FindByName(name).MessageCount;

        // 应用实体由单对象缓存共享，并发生产必须串行累加
        var runs = Enumerable.Range(0, 8).Select(i => Task.Run(() =>
            fixture.JobService.Produce(app, new ProduceModel { Job = jobName, Topic = topic, Messages = [$"m{i}"] }))).ToArray();
        var counts = await Task.WhenAll(runs);

        Assert.Equal(8, counts.Sum());

        var entity = App.FindByName(name);
        Assert.Equal(before + 8, entity.MessageCount);
    }

    [Fact(DisplayName = "申请不存在的作业给出明确错误")]
    public void JobService_Acquire_UnknownJob()
    {
        var name = "SrvUnknown_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var online = fixture.AppService.GetOnline(app, $"{name}@unk", "127.0.0.1");

        // 作业不存在时应抛出明确的业务异常，而不是空引用
        var ex = Assert.Throws<NewLife.XException>(() =>
            fixture.JobService.Acquire(app, new AcquireModel { Job = "NoSuchJob_" + name, Count = 1 }, online));
        Assert.Contains("未找到作业", ex.Message);
    }

    [Fact(DisplayName = "单次申请任务数受上限约束")]
    public void JobService_Acquire_Clamps_Count()
    {
        var name = "SrvClamp_" + Guid.NewGuid().ToString("N")[..6];
        Login(fixture.AppService, name);

        var app = App.FindByName(name);
        var jobName = "ClampJob";
        fixture.JobService.AddJobs(app, [new JobModel { Name = jobName, Mode = JobModes.Time, Enable = true, DataTime = DateTime.Today.AddDays(-5), Step = 30, Offset = 15 }]);

        var online = fixture.AppService.GetOnline(app, $"{name}@clamp", "127.0.0.1");

        // 客户端异常请求超大数量时，服务端必须收敛到上限
        var tasks = fixture.JobService.Acquire(app, new AcquireModel { Job = jobName, Count = 100_000 }, online);

        Assert.True(tasks.Length <= JobService.MaxAcquireCount, $"返回了{tasks.Length}个任务");
        Assert.NotEmpty(tasks);
    }

    [Fact(DisplayName = "清理作业已不存在的任务")]
    public void JobService_ClearItems_DeletesOrphanTask()
    {
        var task = new JobTask { JobID = 999_999_999, DataTime = DateTime.Now, Status = JobStatus.就绪 };
        task.Insert();
        Assert.True(task.ID > 0);

        var count = JobService.ClearItems();
        Assert.True(count >= 1);
        Assert.Null(JobTask.Find(JobTask._.ID == task.ID));
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

    [Fact(DisplayName = "Worker停止后释放监听端口")]
    public async Task Worker_Stop_Releases_Port()
    {
        var setting = new AntJobSetting { Port = 0, AutoRegistry = true, TokenSecret = "HS256:antjob_test_secret" };
        var worker = new Worker(fixture.CacheProvider, BuildProvider(fixture), setting, DefaultTracer.Instance ?? new DefaultTracer());

        await worker.StartAsync(CancellationToken.None);

        // 使用指定端口，停止后应能立即重新监听同一端口
        var port = FreePort();
        var setting2 = new AntJobSetting { Port = port, AutoRegistry = true, TokenSecret = "HS256:antjob_test_secret" };
        var worker2 = new Worker(fixture.CacheProvider, BuildProvider(fixture), setting2, DefaultTracer.Instance ?? new DefaultTracer());
        await worker2.StartAsync(CancellationToken.None);
        await worker2.StopAsync(CancellationToken.None);

        // 端口已释放，可再次绑定
        var worker3 = new Worker(fixture.CacheProvider, BuildProvider(fixture), setting2, DefaultTracer.Instance ?? new DefaultTracer());
        await worker3.StartAsync(CancellationToken.None);

        var ex = await Record.ExceptionAsync(() => worker3.StopAsync(CancellationToken.None));
        Assert.Null(ex);

        await worker.StopAsync(CancellationToken.None);
    }

    private static Int32 FreePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
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
