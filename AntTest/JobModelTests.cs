using System;
using AntJob.Data;
using Xunit;

namespace AntTest;

/// <summary>作业模型测试</summary>
public class JobModelTests
{
    [Fact]
    public void Constructor_Default_BatchSize()
    {
        var model = new JobModel();
        Assert.Equal(5000, model.BatchSize);
    }

    [Fact]
    public void Constructor_Default_MaxTask()
    {
        var model = new JobModel();
        Assert.Equal(0, model.MaxTask);
    }

    [Fact]
    public void Constructor_Default_Enable()
    {
        var model = new JobModel();
        Assert.False(model.Enable);
    }

    [Fact]
    public void Constructor_Default_Mode_IsZero()
    {
        var model = new JobModel();
        // 枚举默认值为0（无对应命名值），Data=1, Time=2, Message=3
        Assert.Equal((JobModes)0, model.Mode);
    }

    [Fact]
    public void SetProperties_All()
    {
        var model = new JobModel
        {
            Name = "TestJob",
            ClassName = "Test.Handler",
            Enable = true,
            DataTime = DateTime.Now,
            End = DateTime.Now.AddHours(1),
            Offset = 15,
            Step = 30,
            BatchSize = 10000,
            MaxTask = 4,
            Mode = JobModes.Time,
            Cron = "0/30 * * * ?",
            Topic = "test",
            Data = "{\"key\":\"value\"}",
            DisplayName = "测试作业",
            Description = "A test job",
        };

        Assert.Equal("TestJob", model.Name);
        Assert.Equal("Test.Handler", model.ClassName);
        Assert.True(model.Enable);
        Assert.Equal(30, model.Step);
        Assert.Equal(15, model.Offset);
        Assert.Equal(10000, model.BatchSize);
        Assert.Equal(4, model.MaxTask);
        Assert.Equal(JobModes.Time, model.Mode);
        Assert.Equal("0/30 * * * ?", model.Cron);
        Assert.Equal("test", model.Topic);
        Assert.Equal("测试作业", model.DisplayName);
        Assert.Equal("A test job", model.Description);
    }

    [Fact]
    public void ToString_Returns_Name()
    {
        var model = new JobModel { Name = "MyJob" };
        Assert.Equal("MyJob", model.ToString());
    }

    [Fact]
    public void Clone_Returns_Copy()
    {
        var model = new JobModel { Name = "Original", Step = 60 };
        var clone = (model as ICloneable).Clone() as JobModel;
        Assert.NotNull(clone);
        Assert.Equal("Original", clone.Name);
        Assert.Equal(60, clone.Step);

        // 修改原对象不影响克隆
        model.Name = "Modified";
        Assert.Equal("Original", clone.Name);
    }
}
