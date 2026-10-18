using System.Collections.Concurrent;
using AntJob.Server.Services;
using NewLife;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Remoting.Services;

namespace AntJob.Web.Services;

/// <summary>蚂蚁通知会话管理器。管理客户端 WebSocket 连接并下发通知</summary>
/// <remarks>
/// 会话编码为 <c>应用名@连接标识</c>。发布方只带应用名，
/// 管理器按前缀分发到本机该应用的所有连接，实现多实例客户端同时收到通知。
/// </remarks>
public class AntNotifyManager : SessionManager
{
    private readonly ConcurrentDictionary<String, ConcurrentDictionary<String, ICommandSession>> _apps = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>实例化通知会话管理器</summary>
    /// <param name="serviceProvider">服务提供者</param>
    public AntNotifyManager(IServiceProvider serviceProvider) : base(serviceProvider) => Topic = AntNotifyTopic.Name;

    /// <summary>添加会话，并按应用建立索引</summary>
    /// <param name="session">命令会话</param>
    public override void Add(ICommandSession session)
    {
        base.Add(session);

        var app = GetApp(session?.Code);
        if (!app.IsNullOrEmpty())
            _apps.GetOrAdd(app, _ => new(StringComparer.OrdinalIgnoreCase))[session.Code] = session;
    }

    /// <summary>移除会话，同时清理应用索引</summary>
    /// <param name="session">命令会话</param>
    public override void Remove(ICommandSession session)
    {
        base.Remove(session);

        var app = GetApp(session?.Code);
        if (!app.IsNullOrEmpty() && _apps.TryGetValue(app, out var bucket))
        {
            bucket.TryRemove(session.Code, out _);
            if (bucket.IsEmpty) _apps.TryRemove(app, out _);
        }
    }

    /// <summary>关闭全部会话，同时清空应用索引</summary>
    /// <param name="reason">原因</param>
    public override void CloseAll(String reason)
    {
        base.CloseAll(reason);

        _apps.Clear();
    }

    private static String GetApp(String code)
    {
        if (code.IsNullOrEmpty()) return null;

        var p = code.IndexOf('@');
        return p > 0 ? code[..p] : code;
    }

    /// <summary>从事件总线收到通知消息，按应用前缀分发给本机会话</summary>
    /// <param name="message">消息。格式为 应用名#JSON</param>
    /// <param name="context">事件上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    protected override async Task OnMessage(String message, IEventContext context, CancellationToken cancellationToken)
    {
        if (message.IsNullOrEmpty()) return;

        var p = message.IndexOf('#');
        if (p <= 0) return;

        var code = message[..p];
        var json = message[(p + 1)..];

        // 按应用索引取本机会话，避免遍历全部连接
        if (!_apps.TryGetValue(code, out var bucket)) return;

        foreach (var item in bucket)
        {
            // 会话可能已被清理或替换，顺手剔除失效项
            if (!Sessions.TryGetValue(item.Key, out var current) || !ReferenceEquals(current, item.Value) || !current.Active)
            {
                bucket.TryRemove(item.Key, out _);
                continue;
            }

            try
            {
                await item.Value.HandleAsync(null, json, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 单个会话发送失败不影响其它会话
                XTrace.Log.Debug("WebSocket通知[{0}]失败：{1}", item.Key, ex.Message);
            }
        }
    }
}
