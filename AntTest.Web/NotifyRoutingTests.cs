extern alias web;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AntJob.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using NewLife;
using NewLife.Caching;
using NewLife.Remoting.Models;
using NewLife.Remoting.Services;
using NewLife.Serialization;
using Xunit;
using AntNotifyManager = web::AntJob.Web.Services.AntNotifyManager;

namespace AntTest.Web;

/// <summary>通知会话路由测试。验证按应用前缀分发到本机所有实例</summary>
public class NotifyRoutingTests
{
    [Fact]
    public async Task Send_To_All_Sessions_Of_App()
    {
        var provider = new CacheProvider();
        var services = new ServiceCollection();
        services.AddSingleton<ICacheProvider>(provider);
        using var sp = services.BuildServiceProvider();

        using var manager = new AntNotifyManager(sp);
        var a = new FakeSession("AppX@a");
        var b = new FakeSession("AppX@b");
        var c = new FakeSession("AppY@c");
        manager.Add(a);
        manager.Add(b);
        manager.Add(c);

        var command = new CommandModel { Command = "Message", Argument = "{\"app\":\"AppX\"}" };
        var bus = provider.CreateEventBus<String>(AntNotifyTopic.Name, Runtime.ClientId);
        Assert.NotNull(bus);
        await bus.PublishAsync($"AppX#{command.ToJson()}");

        var ok = await WaitUntilAsync(() => a.Messages.Count > 0 && b.Messages.Count > 0, 5);
        Assert.True(ok);
        Assert.Empty(c.Messages);
    }

    [Fact(DisplayName = "Redis 多实例广播（需 REDIS_CONNECTION 或本机 6379）")]
    public async Task Broadcast_Across_Two_Managers()
    {
        // 无 Redis 环境直接跳过，避免测试卡在连接超时
        var conn = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
        if (conn.IsNullOrEmpty()) conn = "127.0.0.1:6379";

        using var redis = new FullRedis { Timeout = 1_000 };
        try
        {
            redis.Init(conn);
            redis.Set("antjob:test:ping", 1, 10);
        }
        catch
        {
            return;
        }

        var provider = new CacheProvider { Cache = redis, InnerCache = redis };
        var services = new ServiceCollection();
        services.AddSingleton<ICacheProvider>(provider);
        using var sp = services.BuildServiceProvider();

        // 两个模拟 Web 实例，使用各自消费组
        var topic = AntNotifyTopic.Name + ":" + Guid.NewGuid().ToString("N");
        using var m1 = new AntNotifyManager(sp) { ClientId = "node1", Topic = topic, ClearPeriod = 0 };
        using var m2 = new AntNotifyManager(sp) { ClientId = "node2", Topic = topic, ClearPeriod = 0 };

        var a = new FakeSession("AppX@a");
        var b = new FakeSession("AppX@b");
        m1.Add(a);
        m2.Add(b);

        var command = new CommandModel { Command = "Message", Argument = "{\"app\":\"AppX\"}" };
        var bus = provider.CreateEventBus<String>(topic, "publisher");
        Assert.NotNull(bus);

        // 消费组创建完成前发布的消息会被跳过，循环发布直到两个实例都收到
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while ((a.Messages.Count == 0 || b.Messages.Count == 0) && DateTime.UtcNow < deadline)
        {
            await bus.PublishAsync($"AppX#{command.ToJson()}");
            await Task.Delay(200);
        }

        Assert.True(a.Messages.Count > 0 && b.Messages.Count > 0, "Redis 多消费组未同时收到通知");

        redis.Remove($"antjob:test:ping");
    }

    private static async Task<Boolean> WaitUntilAsync(Func<Boolean> condition, Int32 seconds)
    {
        var end = DateTime.UtcNow.AddSeconds(seconds);
        while (DateTime.UtcNow < end)
        {
            if (condition()) return true;
            await Task.Delay(100);
        }

        return condition();
    }

    private class FakeSession(String code) : ICommandSession
    {
        public String Code { get; set; } = code;

        public Boolean Active => true;

        public List<String> Messages { get; } = [];

        public Task HandleAsync(CommandModel command, String message, CancellationToken cancellationToken)
        {
            Messages.Add(message);

            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
