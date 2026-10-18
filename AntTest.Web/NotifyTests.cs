using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AntJob;
using AntJob.Providers;
using NewLife;
using NewLife.Log;
using Xunit;

namespace AntTest.Web;

/// <summary>WebSocket 下行通知集成测试</summary>
[Collection("Web")]
public class NotifyTests(WebHostFixture fixture)
{
    private HttpClient CreateClient() => new() { BaseAddress = new Uri(fixture.Server) };

    private async Task<(String Token, HttpClient Client)> LoginAsync(String app)
    {
        var client = CreateClient();
        var rs = await client.PostAsJsonAsync("/AntJob/Token", new
        {
            grant_type = "password",
            username = app,
            password = "",
            clientId = "notify-test",
        });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object) root = data;
        var token = root.GetProperty("access_token").GetString();
        Assert.False(token.IsNullOrEmpty());

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        return (token, client);
    }

    private async Task<ClientWebSocket> ConnectAsync(String token, String clientId = null)
    {
        var ws = new ClientWebSocket();
        ws.Options.SetRequestHeader("Authorization", "Bearer " + token);

        var url = fixture.Server.Replace("http://", "ws://") + "/AntJob/Notify";
        if (!clientId.IsNullOrEmpty()) url += "?clientId=" + Uri.EscapeDataString(clientId);

        await ws.ConnectAsync(new Uri(url), CancellationToken.None);

        return ws;
    }

    private static async Task<String> ReceiveAsync(ClientWebSocket ws, Int32 seconds = 5)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        var buf = new Byte[8 * 1024];
        var sb = new StringBuilder();
        while (true)
        {
            var rs = await ws.ReceiveAsync(new ArraySegment<Byte>(buf), cts.Token);
            if (rs.MessageType == WebSocketMessageType.Close) return null;

            sb.Append(Encoding.UTF8.GetString(buf, 0, rs.Count));
            if (rs.EndOfMessage) return sb.ToString();
        }
    }

    [Fact]
    public async Task Notify_JobChanged()
    {
        var app = "NotifyApp_" + Guid.NewGuid().ToString("N")[..6];
        var (token, http) = await LoginAsync(app);
        var job = "NotifyJob";

        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 1, enable = true, dataTime = DateTime.Today, step = 30, offset = 15 },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        using var ws = await ConnectAsync(token);

        // 等待服务端完成会话注册
        await Task.Delay(500);

        var rs = await http.PostAsJsonAsync("/AntJob/SetJob", new { name = job, step = 31 });
        Assert.True(rs.IsSuccessStatusCode, await rs.Content.ReadAsStringAsync());

        var msg = await ReceiveAsync(ws);

        Assert.NotNull(msg);
        Assert.Contains("JobChanged", msg);

        http.Dispose();
    }

    [Fact]
    public async Task Notify_Message()
    {
        var app = "NotifyMsgApp_" + Guid.NewGuid().ToString("N")[..6];
        var (token, http) = await LoginAsync(app);
        var job = "NotifyMsgJob";
        var topic = "T1";

        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 3, topic, enable = true },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        using var ws = await ConnectAsync(token);

        await Task.Delay(500);

        var rs = await http.PostAsJsonAsync("/AntJob/Produce", new { job, topic, messages = new[] { "m1", "m2" } });
        Assert.True(rs.IsSuccessStatusCode, await rs.Content.ReadAsStringAsync());

        var msg = await ReceiveAsync(ws);

        Assert.NotNull(msg);
        Assert.Contains("Message", msg);

        http.Dispose();
    }

    [Fact]
    public async Task Notify_Reconnect_Replaces_Session()
    {
        var app = "NotifyReconnectApp_" + Guid.NewGuid().ToString("N")[..6];
        var (token, http) = await LoginAsync(app);
        var job = "NotifyReconnectJob";
        var clientId = "reconnect-" + Guid.NewGuid().ToString("N")[..6];

        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 1, enable = true, dataTime = DateTime.Today, step = 30, offset = 15 },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        using var ws1 = await ConnectAsync(token, clientId);
        await Task.Delay(500);

        // 同一客户端标识重连，服务端应关闭旧会话并保留新会话
        using var ws2 = await ConnectAsync(token, clientId);
        await Task.Delay(500);

        Assert.True(await IsClosedAsync(ws1, 3), "旧连接未被关闭");

        var rs = await http.PostAsJsonAsync("/AntJob/SetJob", new { name = job, step = 32 });
        Assert.True(rs.IsSuccessStatusCode, await rs.Content.ReadAsStringAsync());

        var msg = await ReceiveAsync(ws2);
        Assert.NotNull(msg);
        Assert.Contains("JobChanged", msg);

        http.Dispose();
    }

    private static async Task<Boolean> IsClosedAsync(ClientWebSocket ws, Int32 seconds)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
        try
        {
            var buf = new Byte[1024];
            var rs = await ws.ReceiveAsync(new ArraySegment<Byte>(buf), cts.Token);
            return rs.MessageType == WebSocketMessageType.Close;
        }
        catch (OperationCanceledException) { return false; }
        catch (WebSocketException) { return true; }
    }

    [Fact]
    public async Task AntClient_Receives_JobChanged()
    {
        var app = "AntClientApp_" + Guid.NewGuid().ToString("N")[..6];
        var (_, http) = await LoginAsync(app);
        var job = "AntClientJob";

        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 1, enable = true, dataTime = DateTime.Today, step = 30, offset = 15 },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        var setting = new AntSetting
        {
            Server = fixture.Server,
            AppId = app,
            Secret = "",
        };
        using var client = new AntClient(setting) { Log = XTrace.Log };

        var tcs = new TaskCompletionSource<String>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Received += (s, e) => tcs.TrySetResult(e.Model?.Command);

        await client.Login();

        // 首次心跳延迟3秒后建立WebSocket，循环触发直到收到通知，避免依赖固定等待时间
        var deadline = DateTime.UtcNow.AddSeconds(15);
        var step = 30;
        while (!tcs.Task.IsCompleted && DateTime.UtcNow < deadline)
        {
            step = step == 30 ? 31 : 30;
            await http.PostAsJsonAsync("/AntJob/SetJob", new { name = job, step });
            await Task.Delay(1_000);
        }

        var command = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal("JobChanged", command);
        Assert.True(client.NotifyConnected);

        http.Dispose();
    }
}
