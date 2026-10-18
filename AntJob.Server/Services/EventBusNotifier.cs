using AntJob.Data.Entity;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Remoting.Models;
using NewLife.Serialization;

namespace AntJob.Server.Services;

/// <summary>基于事件总线的作业通知器</summary>
/// <remarks>
/// 发布到 <see cref="AntNotifyTopic.Name"/> 主题，消息格式为 <c>应用名#CommandModel</c>。
/// 单机模式使用内存队列，集群模式（配置 Redis）时由 ICacheProvider 创建 Redis Stream 总线，
/// 每个 Web 实例以自身 ClientId 作为消费组，各自收到一份，实现跨实例广播。
/// </remarks>
public class EventBusNotifier : IJobNotifier
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;
    private readonly ILog _log;

    private IEventBus<String> _bus;

    /// <summary>实例化通知器，并订阅作业变更事件</summary>
    /// <param name="cacheProvider">缓存提供者，用于创建事件总线与去抖窗口</param>
    /// <param name="tracer">性能跟踪器</param>
    /// <param name="log">日志</param>
    public EventBusNotifier(ICacheProvider cacheProvider, ITracer tracer, ILog log)
    {
        _cacheProvider = cacheProvider;
        _tracer = tracer;
        _log = log;

        Job.Changed += OnJobChanged;
    }

    #region 发布
    /// <summary>通知作业调度参数已变更</summary>
    /// <param name="app">应用名</param>
    /// <param name="job">作业名</param>
    public void NotifyJobChanged(String app, String job)
    {
        if (app.IsNullOrEmpty() || job.IsNullOrEmpty()) return;

        // 同一作业在短时间内多次变更时合并，避免通知风暴
        if (!_cacheProvider.Cache.Add($"antjob:notify:{app}:{job}", 1, 1)) return;

        var argument = new Dictionary<String, Object>
        {
            ["app"] = app,
            ["job"] = job,
        };

        _ = PublishAsync(app, "JobChanged", argument.ToJson());
    }

    /// <summary>通知有新消息到达</summary>
    /// <param name="app">应用名</param>
    /// <param name="topic">主题</param>
    /// <param name="count">消息数</param>
    public void NotifyMessage(String app, String topic, Int32 count)
    {
        if (app.IsNullOrEmpty() || topic.IsNullOrEmpty() || count <= 0) return;

        if (!_cacheProvider.Cache.Add($"antjob:notify:{app}:{topic}", 1, 1)) return;

        var argument = new Dictionary<String, Object>
        {
            ["app"] = app,
            ["topic"] = topic,
            ["count"] = count,
        };

        _ = PublishAsync(app, "Message", argument.ToJson());
    }

    private async Task PublishAsync(String code, String command, String argument)
    {
        using var span = _tracer?.NewSpan($"antjob:notify:{command}", $"{code} {argument}");
        try
        {
            var model = new CommandModel
            {
                Command = command,
                Argument = argument,
                TraceId = DefaultSpan.Current?.TraceId,
            };

            var message = $"{code}#{model.ToJson()}";

            _bus ??= _cacheProvider.CreateEventBus<String>(AntNotifyTopic.Name, Runtime.ClientId);
            await _bus.PublishAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            _log?.Error("发布作业通知[{0}]失败：{1}", command, ex.Message);
        }
    }
    #endregion

    #region 事件
    private void OnJobChanged(Object sender, JobChangedEventArgs e)
    {
        var job = e.Job;
        if (job == null) return;

        var app = job.App;
        if (app == null) return;

        NotifyJobChanged(app.Name, job.Name);
    }
    #endregion
}
