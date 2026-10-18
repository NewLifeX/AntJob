using System;
using System.IO;
using System.Linq;
using AntJob;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Extensions;
using NewLife;
using NewLife.Log;
using XCode;
using XCode.DataAccessLayer;
using Xunit;

namespace AntTest;

/// <summary>数据层集成测试（SQLite 文件模式）</summary>
[Collection("Integration")]
public class DataLayerTests
{
    public DataLayerTests()
    {
        XTrace.UseConsole();
        TestDb.Init();
    }

    [Fact]
    public void App_Create_And_Find()
    {
        var app = new App
        {
            Name = "TestApp",
            DisplayName = "测试应用",
            Enable = true,
        };

        var count = app.Insert();
        Assert.True(count > 0);
        Assert.True(app.ID > 0);

        var found = App.FindByID(app.ID);
        Assert.NotNull(found);
        Assert.Equal("TestApp", found.Name);
        Assert.Equal("测试应用", found.DisplayName);
        Assert.True(found.Enable);
    }

    [Fact]
    public void App_FindByName()
    {
        var app = new App { Name = "UniqueApp", Enable = true };
        app.Insert();

        var found = App.FindByName("UniqueApp");
        Assert.NotNull(found);
        Assert.Equal("UniqueApp", found.Name);
    }

    [Fact]
    public void Job_Create_And_Query()
    {
        // 先创建关联的应用
        var app = new App { Name = "JobApp", Enable = true };
        app.Insert();

        var job = new Job
        {
            AppID = app.ID,
            Name = "TestJob",
            Mode = AntJob.Data.JobModes.Time,
            DataTime = DateTime.Today,
            Enable = true,
        };
        job.Insert();

        Assert.True(job.ID > 0);

        // 按应用查询
        var jobs = Job.FindAllByAppID(app.ID);
        Assert.NotNull(jobs);
        Assert.Single(jobs);
        Assert.Equal("TestJob", jobs[0].Name);
    }

    [Fact]
    public void Job_CheckQuiet_Integration()
    {
        var app = new App { Name = "QuietApp", Enable = true };
        app.Insert();

        var job = new Job
        {
            AppID = app.ID,
            Name = "QuietJob",
            QuietTime = "09:00-12:00",
            Enable = true,
        };
        job.Insert();

        // 在免打扰时段内
        var inside = DateTime.Today.AddHours(10); // 10:00
        Assert.True(job.CheckQuiet(inside));

        // 在免打扰时段外
        var outside = DateTime.Today.AddHours(14); // 14:00
        Assert.False(job.CheckQuiet(outside));
    }

    [Fact]
    public void JobTask_Create_And_Query()
    {
        var app = new App { Name = "TaskApp", Enable = true };
        app.Insert();

        var job = new Job { AppID = app.ID, Name = "SourceJob", Enable = true };
        job.Insert();

        var task = new JobTask
        {
            AppID = app.ID,
            JobID = job.ID,
            DataTime = DateTime.Now,
            Status = AntJob.Data.JobStatus.就绪,
        };
        task.Insert();

        Assert.True(task.ID > 0);

        // 按应用查询
        var tasks = JobTask.FindAllByAppID(app.ID);
        Assert.NotNull(tasks);
        Assert.Single(tasks);
        Assert.Equal(job.ID, tasks[0].JobID);
    }

    [Fact]
    public void AppOnline_Create_And_Update()
    {
        var app = new App { Name = "OnlineApp", Enable = true };
        app.Insert();

        var online = new AppOnline
        {
            AppID = app.ID,
            Instance = "192.168.1.1:9999",
            Name = "Server01",
            Enable = true,
        };
        online.Insert();

        Assert.True(online.ID > 0);

        // 更新统计
        online.Tasks = 10;
        online.Total = 1000;
        online.Success = 999;
        online.Update();

        var found = AppOnline.FindByID(online.ID);
        Assert.NotNull(found);
        Assert.Equal(10, found.Tasks);
        Assert.Equal(1000, found.Total);
        Assert.Equal(999, found.Success);
    }

    [Fact]
    public void DataHandler_Init_SetsFactory()
    {
        var handler = new TestAppHandler();
        handler.Init();

        Assert.NotNull(handler.Factory);
        Assert.Equal(JobModes.Data, handler.Mode);
    }

    [Fact]
    public void DataHandler_Init_AutoDetectsField()
    {
        var handler = new TestAppHandler();
        handler.Init();

        // App 表没有雪花Id，没有MasterTime，应该使用 CreateTime
        Assert.NotNull(handler.Field);
    }

    [Fact]
    public void DataHandler_Init_GetMinDataTime()
    {
        // 先插入测试数据
        for (var i = 0; i < 3; i++)
        {
            new App { Name = $"BulkApp_{i}", Enable = true }.Insert();
        }

        var handler = new TestAppHandler();
        handler.Init();

        var min = handler.GetMinDataTime();
        Assert.NotEqual(DateTime.MinValue, min);
        Assert.True(min.Year > 2000);
    }

    [Fact]
    public void DataHandler_Execute_ReturnsValue()
    {
        var handler = new TestAppHandler();
        handler.Init();

        var ctx = new JobContext
        {
            Handler = handler,
            Task = new TaskModel(),
        };

        var result = handler.Execute(ctx);
        Assert.Equal(1, result);
    }
}

/// <summary>测试用 App 数据处理器（DataHandler 集成测试用）</summary>
internal class TestAppHandler : AntJob.Extensions.DataHandler<App>
{
    /// <summary>执行</summary>
    public override Int32 Execute(JobContext ctx) => 1;
}
