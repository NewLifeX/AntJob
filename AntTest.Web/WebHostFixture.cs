extern alias web;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife;
using NewLife.Log;
using Xunit;

namespace AntTest.Web;

/// <summary>Web 宿主机测试夹具。以 Kestrel 随机端口启动 AntJob.Web，供 HTTP API 集成测试共享</summary>
public sealed class WebHostFixture : IAsyncLifetime
{
    private IHost _host;

    /// <summary>服务地址，如 http://127.0.0.1:5000</summary>
    public String Server { get; private set; }

    public async Task InitializeAsync()
    {
        XTrace.UseConsole();

        TestDb.Init();

        // 令牌密钥，测试环境显式指定
        web::AntJob.Server.AntJobSetting.Current.TokenSecret = "HS256:antjob_test_secret";

        var builder = web::AntJob.Web.Program.CreateHostBuilder([])
            .UseContentRoot(Path.GetDirectoryName(typeof(web::AntJob.Web.Startup).Assembly.Location))
            .ConfigureWebHost(wb =>
            {
                wb.UseUrls("http://127.0.0.1:0");
                // 测试宿主进程是 AntTest，需显式指定应用名，才能扫描到 AntJob.Web 内的控制器
                wb.UseSetting(WebHostDefaults.ApplicationKey, typeof(web::AntJob.Web.Startup).Assembly.GetName().Name);
            });
        _host = builder.Build();

        await _host.StartAsync();

        var addresses = _host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses;
        Server = addresses?.FirstOrDefault()?.Replace("[::]", "127.0.0.1").Replace("localhost", "127.0.0.1");
    }

    public Task DisposeAsync()
    {
        // 不停止宿主机：宿主关停会清理全局定时器与全局服务提供者，影响同进程内其它集成测试
        // 测试进程退出时随进程释放

        return Task.CompletedTask;
    }
}

/// <summary>Web 集成测试集合。共享同一个宿主机实例</summary>
[CollectionDefinition("Web")]
public class WebCollection : ICollectionFixture<WebHostFixture> { }

/// <summary>Web 宿主启动冒烟测试</summary>
[Collection("Web")]
public class WebHostSmokeTests(WebHostFixture fixture)
{
    [Fact]
    public void Host_Started()
    {
        Assert.False(fixture.Server.IsNullOrEmpty());
    }
}

