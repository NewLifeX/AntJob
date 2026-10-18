using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AntJob.Data.Entity;
using Xunit;

namespace AntTest.Web;

/// <summary>HTTP 通道身份识别集成测试</summary>
[Collection("Web")]
public class HttpIdentityTests(WebHostFixture fixture)
{
    private HttpClient CreateClient() => new() { BaseAddress = new Uri(fixture.Server) };

    [Fact(DisplayName = "HTTP申请任务绑定登录时的在线实例")]
    public async Task Acquire_Uses_Login_ClientId()
    {
        var app = "HttpIdentity_" + Guid.NewGuid().ToString("N")[..6];
        var clientId = "instance-a";
        using var http = CreateClient();

        var login = await http.PostAsJsonAsync("/AntJob/Token", new
        {
            grant_type = "password",
            username = app,
            password = "",
            clientId,
        });
        var body = await login.Content.ReadAsStringAsync();
        Assert.True(login.IsSuccessStatusCode, body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object) root = data;
        http.DefaultRequestHeaders.Authorization = new("Bearer", root.GetProperty("access_token").GetString());

        var job = "IdentityJob";
        var add = await http.PostAsJsonAsync("/AntJob/AddJobs", new[]
        {
            new { name = job, mode = 1, enable = true, dataTime = DateTime.Today.AddHours(-2), step = 30, offset = 15 },
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());

        var acq = await http.PostAsJsonAsync($"/AntJob/Acquire?clientId={clientId}", new { job, count = 1 });
        Assert.True(acq.IsSuccessStatusCode, await acq.Content.ReadAsStringAsync());

        var appEntity = App.FindByName(app);
        Assert.NotNull(appEntity);
        var jobEntity = Job.FindByAppIDAndName(appEntity.ID, job);
        Assert.NotNull(jobEntity);

        // 任务记录应绑定登录时的客户端标识，而不是退化成按IP生成的实例
        var task = JobTask.Find(JobTask._.JobID == jobEntity.ID);
        Assert.NotNull(task);
        Assert.Equal(clientId, task.Client);
    }
}
