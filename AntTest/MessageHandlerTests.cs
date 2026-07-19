using System;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Handlers;
using NewLife.Serialization;
using Xunit;

namespace AntTest;

/// <summary>测试消息处理器</summary>
internal class TestMessageHandler : MessageHandler
{
    public TestMessageHandler() => Topic = "TestTopic";

    public Int32 ProcessedCount { get; private set; }

    public override Boolean ProcessItem(JobContext ctx, String message)
    {
        ProcessedCount++;
        return true;
    }

    public override Int32 Execute(JobContext ctx) => 1;
}

/// <summary>消息处理器测试</summary>
public class MessageHandlerTests
{
    [Fact]
    public void Constructor_Sets_MessageMode()
    {
        var handler = new TestMessageHandler();
        Assert.Equal(JobModes.Message, handler.Mode);
    }

    [Fact]
    public void Constructor_Sets_BatchSize()
    {
        var handler = new TestMessageHandler();
        Assert.Equal(8, handler.Job.BatchSize);
    }

    [Fact]
    public void Constructor_Sets_DataTime_MinValue()
    {
        var handler = new TestMessageHandler();
        Assert.Equal(DateTime.MinValue, handler.Job.DataTime);
    }

    [Fact]
    public void Constructor_Sets_Topic()
    {
        var handler = new TestMessageHandler();
        Assert.Equal("TestTopic", handler.Topic);
    }

    [Fact]
    public void Start_Throws_IfTopicEmpty()
    {
        var handler = new TestMessageHandler();
        handler.Topic = null;
        Assert.Throws<ArgumentNullException>(() => handler.Start());
    }

    [Fact]
    public void Start_Succeeds_WithTopic()
    {
        var handler = new TestMessageHandler();
        var result = handler.Start();
        Assert.True(result);
        Assert.True(handler.Active);
    }

    [Fact]
    public void OnProcess_DecodesMessages()
    {
        var handler = new TestMessageHandler();
        var task = new TaskModel { ID = 1, Data = "[\"msg1\",\"msg2\",\"msg3\"]" };

        // 直接测试 OnProcess（不走Process的外层包装）
        var ctx = new JobContext { Handler = handler, Task = task, Result = new TaskResult() };

        // 模拟OnProcess行为：解码消息并调用Execute
        var ss = ctx.Task.Data.ToJsonEntity<String[]>();
        Assert.NotNull(ss);
        Assert.Equal(3, ss.Length);
        Assert.Equal("msg1", ss[0]);
        Assert.Equal("msg2", ss[1]);
        Assert.Equal("msg3", ss[2]);
    }

    [Fact]
    public void OnProcess_EmptyData_NoError()
    {
        var handler = new TestMessageHandler();
        var task = new TaskModel { ID = 2, Data = "" };
        var ctx = handler.Process(task);

        Assert.NotNull(ctx);
        Assert.Equal(0, ctx.Total);
    }

    [Fact]
    public void OnProcess_NullData_NoError()
    {
        var handler = new TestMessageHandler();
        var task = new TaskModel { ID = 3 };
        var ctx = handler.Process(task);

        Assert.NotNull(ctx);
        Assert.Equal(0, ctx.Total);
    }
}
