using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AntJob;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using Xunit;

namespace AntTest.Web;

/// <summary>HTTP + WebSocket 端到端测试：服务端变更经 WS 通知后，客户端立即拉取最新作业</summary>
[Collection("Web")]
public class HttpNotifyRefreshTests(WebHostFixture fixture)
{
    private async Task<(String Token, HttpClient Client)> LoginAsync(String app)
    {
        var client = new HttpClient { BaseAddress = new Uri(fixture.Server) };
        var rs = await client.PostAsJsonAsync("/AntJob/Token", new
        {
            grant_type = "password",
            username = app,
            password = "",
            clientId = "refresh-test",
        });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object) root = data;
        client.DefaultRequestHeaders.Authorization = new("Bearer", root.GetProperty("access_token").GetString());

        return (root.GetProperty("access_token").GetString(), client);
    }

    [Fact(DisplayName = "WS通知后立即拉取到最新作业")]
    public async Task Notify_Triggers_HttpRefresh()
    {
        var app = "HttpRefresh_" + Guid.NewGuid().ToString("N")[..6];
        var (_, http) = await LoginAsync(app);
        var job = "RefreshJob";

        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 1, enable = true, dataTime = DateTime.Today, step = 30, offset = 15 },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        var setting = new AntSetting { Server = fixture.Server, AppId = app, Secret = "" };
        using var provider = new HttpJobProvider(setting) { Schedule = new Scheduler() };
        await provider.Start();

        try
        {
            var client = provider.Ant;
            Assert.NotNull(client);

            // 首次拉取会进入30秒缓存窗口
            var jobs = await provider.GetJobs();
            Assert.Equal(30, jobs.First(e => e.Name == job).Step);

            // 等待下行通道建立，否则变更通知可能早于连接
            var deadline = DateTime.UtcNow.AddSeconds(20);
            while (!client.NotifyConnected && DateTime.UtcNow < deadline)
            {
                await Task.Delay(200);
            }
            Assert.True(client.NotifyConnected, "下行通知通道未连接");

            var received = new TaskCompletionSource<String>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.Received += (s, e) => received.TrySetResult(e.Model?.Command);

            var rs = await http.PostAsJsonAsync("/AntJob/SetJob", new { name = job, step = 45 });
            Assert.True(rs.IsSuccessStatusCode, await rs.Content.ReadAsStringAsync());

            var command = await received.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.Equal("JobChanged", command);

            // 收到通知后缓存失效，无需等待30秒轮询即可看到新参数
            jobs = await provider.GetJobs();
            Assert.Equal(45, jobs.First(e => e.Name == job).Step);
        }
        finally
        {
            await provider.Stop();
            http.Dispose();
        }
    }
}
