using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

/// <summary>AntJob.Web HTTP API 集成测试（真实 Kestrel + SQLite）</summary>
[Collection("Web")]
public class WebApiTests(WebHostFixture fixture)
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    private HttpClient CreateClient() => new() { BaseAddress = new Uri(fixture.Server) };

    private static String GetString(JsonElement root, params String[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String) return value.GetString();
        }

        return null;
    }

    private async Task<(String Token, HttpClient Client)> LoginAsync(String app)
    {
        var client = CreateClient();
        var rs = await client.PostAsJsonAsync("/AntJob/Token", new
        {
            grant_type = "password",
            username = app,
            password = "",
            clientId = "anttest",
        });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object) root = data;
        var token = GetString(root, "access_token", "accessToken", "AccessToken");
        Assert.False(token.IsNullOrEmpty(), body);

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        return (token, client);
    }

    private static async Task<HttpResponseMessage> AddJobsAsync(HttpClient client, params Object[] jobs) =>
        await client.PostAsJsonAsync("/AntJob/AddJobs", jobs);

    [Fact]
    public async Task Token_ReturnsAccessToken()
    {
        var (token, client) = await LoginAsync("WebTokenApp");
        Assert.False(token.IsNullOrEmpty());

        client.Dispose();
    }

    [Fact]
    public async Task Request_Without_Token_Returns_403()
    {
        using var client = CreateClient();

        var rs = await client.GetAsync("/AntJob/GetJobs");
        var body = await rs.Content.ReadAsStringAsync();

        Assert.Contains("403", body);
    }

    [Fact]
    public async Task GetPeers_Returns_Array()
    {
        var (_, client) = await LoginAsync("WebPeerApp");

        var body = await client.GetStringAsync("/AntJob/GetPeers");
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data)) root = data;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);

        client.Dispose();
    }

    [Fact]
    public async Task AddJobs_Then_GetJobs()
    {
        var (_, client) = await LoginAsync("WebJobApp");

        var rs = await AddJobsAsync(client, new { name = "WebJobA", mode = 1, dataTime = DateTime.Today, step = 30, offset = 15 });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        var json = await client.GetStringAsync("/AntJob/GetJobs");
        Assert.Contains("WebJobA", json, StringComparison.OrdinalIgnoreCase);

        client.Dispose();
    }

    [Fact]
    public async Task Acquire_Returns_Tasks()
    {
        var (_, client) = await LoginAsync("WebAcquireApp");

        await AddJobsAsync(client, new { name = "WebAcquireJob", mode = 1, enable = true, dataTime = DateTime.Today.AddHours(-2), step = 30, offset = 15, maxTask = 4 });

        var rs = await client.PostAsJsonAsync("/AntJob/Acquire", new { job = "WebAcquireJob", topic = "", count = 2 });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data)) root = data;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);

        client.Dispose();
    }

    [Fact]
    public async Task Produce_Returns_MessageCount()
    {
        var (_, client) = await LoginAsync("WebProduceApp");

        await AddJobsAsync(client, new { name = "WebProduceJob", mode = 3, topic = "T1" });

        var rs = await client.PostAsJsonAsync("/AntJob/Produce", new
        {
            job = "WebProduceJob",
            topic = "T1",
            messages = new[] { "m1", "m2", "m1" },
        });
        var body = await rs.Content.ReadAsStringAsync();
        Assert.True(rs.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data)) root = data;

        Assert.True(root.GetInt32() >= 2, body);

        client.Dispose();
    }

    [Theory]
    [InlineData("/Ant/App")]
    [InlineData("/Ant/AppOnline")]
    [InlineData("/Ant/Job")]
    [InlineData("/Ant/JobTask")]
    [InlineData("/Ant/AppHistory")]
    [InlineData("/Ant/AppMessage")]
    [InlineData("/Ant/JobError")]
    public async Task Cube_Pages_Respond(String path)
    {
        using var client = CreateClient();
        var rs = await client.GetAsync(path);

        // 未登录时 Cube 会跳转登录页，关键是不能出现 404/500
        Assert.NotEqual(HttpStatusCode.NotFound, rs.StatusCode);
        Assert.True((Int32)rs.StatusCode < 500, $"{path} -> {(Int32)rs.StatusCode}");
    }

    [Fact]
    public async Task HttpJobProvider_EndToEnd()
    {
        var provider = new HttpJobProvider
        {
            Server = fixture.Server,
            AppId = "HttpJobApp_" + Guid.NewGuid().ToString("N")[..6],
            Secret = "",
        };

        var scheduler = new Scheduler();
        scheduler.AddHandler(new HttpTestHandler());

        provider.Schedule = scheduler;
        provider.Init();
        await provider.Start();

        var jobs = await provider.GetJobs();
        Assert.NotNull(jobs);
        Assert.Contains(jobs!, e => e.Name == "HttpTest");

        // 生产消息并申请任务
        var count = await provider.Produce("HttpTest", "T1", new[] { "a", "b" });
        Assert.True(count >= 1);

        var job = jobs!.First(e => e.Name == "HttpTest");
        var task = await provider.SetJob(job);
        Assert.NotNull(task);

        await provider.Stop();
    }
}

/// <summary>HTTP 提供者测试用处理器</summary>
internal class HttpTestHandler : Handler
{
    public override Int32 Execute(JobContext ctx) => 1;
}

