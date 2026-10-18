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
