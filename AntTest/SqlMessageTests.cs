using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Extensions;
using AntJob.Handlers;
using AntJob.Providers;
using NewLife.Data;
using Xunit;

namespace AntTest;

/// <summary>SQL消息处理器测试（纯逻辑，无数据库依赖）</summary>
public class SqlMessageTests
{
    [Fact]
    public void ProduceMessage_NullDt_NoError()
    {
        var ctx = new JobContext();
        // 不应抛出异常
        SqlMessage.ProduceMessage(null, ctx);
    }

    [Fact]
    public void ProduceMessage_EmptyDt_NoError()
    {
        var ctx = new JobContext();
        var dt = new DbTable();
        SqlMessage.ProduceMessage(dt, ctx);
    }

    [Fact]
    public void ProduceMessage_NoTopicColumn_NoProduce()
    {
        var handler = new TestProduceHandler();
        var ctx = new JobContext { Handler = handler };

        var dt = new DbTable();
        dt.Columns = ["id", "name"];
        dt.Rows =
        [
            new object[] { 1, "alice" },
            new object[] { 2, "bob" },
        ];

        SqlMessage.ProduceMessage(dt, ctx);

        // 没有 topic_ 列，不应调用 Produce
        Assert.Empty(handler.ProducedTopics);
    }

    [Fact]
    public void ProduceMessage_TopicColumn_ProducesMessages()
    {
        var handler = new TestProduceHandler();
        var ctx = new JobContext { Handler = handler };

        var dt = new DbTable();
        dt.Columns = ["id", "topic_roleId"];
        dt.Rows =
        [
            new object[] { 1, 101 },
            new object[] { 2, 102 },
            new object[] { 3, 101 }, // 重复值，应去重
        ];

        SqlMessage.ProduceMessage(dt, ctx);

        Assert.Single(handler.ProducedTopics);
        Assert.Equal("roleId", handler.ProducedTopics[0].topic);
        Assert.Equal(2, handler.ProducedTopics[0].messages.Length);
        Assert.Contains("101", handler.ProducedTopics[0].messages);
        Assert.Contains("102", handler.ProducedTopics[0].messages);
    }

    [Fact]
    public void ProduceMessage_MultiTopicColumns()
    {
        var handler = new TestProduceHandler();
        var ctx = new JobContext { Handler = handler };

        var dt = new DbTable();
        dt.Columns = ["topic_roleId", "topic_userId", "name"];
        dt.Rows =
        [
            new object[] { 1, 10, "alice" },
        ];

        SqlMessage.ProduceMessage(dt, ctx);

        Assert.Equal(2, handler.ProducedTopics.Count);
        Assert.Contains(handler.ProducedTopics, p => p.topic == "roleId");
        Assert.Contains(handler.ProducedTopics, p => p.topic == "userId");
    }

    [Fact]
    public void ProduceMessage_Unique_DefaultTrue()
    {
        var handler = new TestProduceHandler();
        var ctx = new JobContext { Handler = handler };

        var dt = new DbTable();
        dt.Columns = ["topic_test"];
        dt.Rows = [new object[] { "msg1" }];

        SqlMessage.ProduceMessage(dt, ctx);

        Assert.Single(handler.ProducedTopics);
        Assert.True(handler.ProducedTopics[0].option.Unique);
    }
}

/// <summary>测试用处理器，记录 Produce 调用</summary>
internal class TestProduceHandler : MessageHandler
{
    public TestProduceHandler() => Topic = "test";

    public List<(String topic, String[] messages, MessageOption option)> ProducedTopics { get; } = [];

    public override Task<Int32> Produce(String topic, String[] messages, MessageOption option = null)
    {
        ProducedTopics.Add((topic, messages, option));
        return Task.FromResult(messages.Length);
    }

    public override Int32 Execute(JobContext ctx) => 0;
}
