using NewLife.Remoting;

namespace AntJob.Providers;

/// <summary>Http任务提供者。通过 HTTP REST API 与调度中心通信，适用于无 TCP 端口的环境</summary>
/// <remarks>
/// 与 NetworkJobProvider（TCP RPC）共用同一套客户端与作业流程，仅通信协议不同。
/// 地址以 http/https 开头时，AntClient 自动选择 HTTP 通道，并通过 WebSocket 接收下行通知。
/// 需配合 AntJob.Web 提供的 /AntJob/* HTTP API 使用。
/// </remarks>
public class HttpJobProvider : NetworkJobProvider
{
    #region 属性
    /// <summary>调试，打开编码日志</summary>
    public Boolean Debug { get => Setting.Debug; set => Setting.Debug = value; }

    /// <summary>调度中心地址</summary>
    public String Server { get => Setting.Server; set => Setting.Server = value; }

    /// <summary>应用编号</summary>
    public String AppId { get => Setting.AppId; set => Setting.AppId = value; }

    /// <summary>应用密钥</summary>
    public String Secret { get => Setting.Secret; set => Setting.Secret = value; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public HttpJobProvider() : base(new AntSetting()) { }

    /// <summary>实例化</summary>
    /// <param name="setting">配置</param>
    public HttpJobProvider(AntSetting setting) : base(setting) { }

    /// <summary>初始化。连接与作业注册统一在 Start 中完成，保留该方法用于兼容旧调用</summary>
    public void Init() { }

    /// <summary>创建客户端。优先复用外部注入的 HTTP 客户端</summary>
    /// <param name="setting">配置</param>
    /// <returns></returns>
    protected override AntClient CreateClient(AntSetting setting)
    {
        var client = base.CreateClient(setting);
        if (Client != null) client.Client = Client;

        return client;
    }
    #endregion

    #region 属性（兼容旧版）
    private ApiHttpClient _client;
    /// <summary>HTTP客户端。旧版属性，保留以兼容外部注入</summary>
    public ApiHttpClient Client
    {
        get => Ant?.Client as ApiHttpClient ?? _client;
        set
        {
            _client = value;
            if (Ant != null) Ant.Client = value;
        }
    }

    #endregion
}
