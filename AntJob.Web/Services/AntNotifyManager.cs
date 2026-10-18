using AntJob.Server.Services;
using NewLife;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Remoting.Models;
using NewLife.Remoting.Services;
using NewLife.Serialization;

namespace AntJob.Web.Services;

/// <summary>蚂蚁通知会话管理器。管理客户端 WebSocket 连接并下发通知</summary>
/// <remarks>
/// 会话编码为 <c>应用名@连接标识</c>。发布方只带应用名，
/// 管理器按前缀分发到本机该应用的所有连接，实现多实例客户端同时收到通知。
/// </remarks>
public class AntNotifyManager : SessionManager
{
    /// <summary>实例化通知会话管理器</summary>
    /// <param name="serviceProvider">服务提供者</param>
    public AntNotifyManager(IServiceProvider serviceProvider) : base(serviceProvider) => Topic = AntNotifyTopic.Name;

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

        CommandModel command = null;
        try
        {
            command = json.ToJsonEntity<CommandModel>();
        }
        catch
        {
            // 命令解析失败时仍然转发原始消息，由客户端决定如何处理
        }

        var prefix = code + "@";
        foreach (var item in Sessions)
        {
            if (!item.Key.EqualIgnoreCase(code) && !item.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                await item.Value.HandleAsync(command, json, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 单个会话发送失败不影响其它会话
                XTrace.Log.Debug("WebSocket通知[{0}]失败：{1}", item.Key, ex.Message);
            }
        }
    }
}
