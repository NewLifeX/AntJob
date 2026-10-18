namespace AntJob.Server.Services;

/// <summary>作业通知器。向在线客户端下发“变更/消息到达”信号</summary>
/// <remarks>
/// 信号只用于降低延迟，不是事实源。客户端收到信号后通过 HTTP 拉取权威数据，
/// 因此通知允许丢失，无需持久化与重放。
/// </remarks>
public interface IJobNotifier
{
    /// <summary>通知作业调度参数已变更</summary>
    /// <param name="app">应用名</param>
    /// <param name="job">作业名</param>
    void NotifyJobChanged(String app, String job);

    /// <summary>通知有新消息到达</summary>
    /// <param name="app">应用名</param>
    /// <param name="topic">主题</param>
    /// <param name="count">消息数</param>
    void NotifyMessage(String app, String topic, Int32 count);
}

/// <summary>通知主题。服务端发布与 Web 端会话管理器共用</summary>
public static class AntNotifyTopic
{
    /// <summary>事件总线主题</summary>
    public const String Name = "antjob:notify";
}
