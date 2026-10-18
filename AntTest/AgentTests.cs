using System;
using System.Linq;
using AntJob;
using AntJob.Extensions;
using NewLife.Model;
using Xunit;

namespace AntTest;

/// <summary>蚂蚁代理入口组装测试（AGT-1）</summary>
[Collection("Integration")]
public class AgentTests
{
    [Fact]
    public void Agent_Composition_Registers_Handlers()
    {
        var set = AntSetting.Current;
        var oldServer = set.Server;
        var oldAppId = set.AppId;
        try
        {
            set.Server = "tcp://127.0.0.1:1";
            set.AppId = "AgentTestApp";

            // 与 AntJob.Agent/Program.cs 保持一致的组装方式
            // 与 Program.cs 一致，使用全局对象容器
            var services = ObjectContainer.Current;
            var scheduler = services.AddAntJob()
                .AddHandler<SqlHandler>()
                .AddHandler<SqlMessage>();

            Assert.NotNull(scheduler);

            var provider = services.BuildServiceProvider();
            scheduler.ServiceProvider = provider;

            var handlers = provider.GetServices<Handler>().ToArray();
            Assert.Contains(handlers, e => e is SqlHandler);
            Assert.Contains(handlers, e => e is SqlMessage);
        }
        finally
        {
            set.Server = oldServer;
            set.AppId = oldAppId;
        }
    }

    [Fact]
    public void Scheduler_AddHandler_Adds_Instance()
    {
        var scheduler = new Scheduler();
        scheduler.AddHandler(new AgentTestHandler());

        Assert.Single(scheduler.Handlers);
        Assert.Equal("AgentTest", scheduler.Handlers[0].Name);
    }
}

/// <summary>代理测试用处理器</summary>
internal class AgentTestHandler : Handler
{
    public override Int32 Execute(JobContext ctx) => 1;
}
