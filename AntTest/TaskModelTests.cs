using System;
using AntJob.Data;
using Xunit;

namespace AntTest;

/// <summary>任务模型测试</summary>
public class TaskModelTests
{
    [Fact]
    public void Constructor_Default()
    {
        var task = new TaskModel();
        Assert.Equal(0, task.ID);
        Assert.Equal(0, task.BatchSize);
        Assert.Null(task.Data);
    }

    [Fact]
    public void SetProperties()
    {
        var now = DateTime.Now;
        var task = new TaskModel
        {
            ID = 42,
            DataTime = now,
            End = now.AddSeconds(30),
            BatchSize = 5000,
            Data = "test data",
        };

        Assert.Equal(42, task.ID);
        Assert.Equal(now, task.DataTime);
        Assert.Equal(now.AddSeconds(30), task.End);
        Assert.Equal(5000, task.BatchSize);
        Assert.Equal("test data", task.Data);
    }

    [Fact]
    public void DataTime_Default()
    {
        var task = new TaskModel();
        Assert.Equal(default, task.DataTime);
    }

    [Fact]
    public void End_Default()
    {
        var task = new TaskModel();
        Assert.Equal(default, task.End);
    }
}
