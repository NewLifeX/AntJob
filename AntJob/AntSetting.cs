using System.ComponentModel;
using System.Reflection;
using NewLife;
using NewLife.Configuration;
using NewLife.Remoting.Clients;

namespace AntJob;

/// <summary>蚂蚁配置。主要用于网络型调度系统</summary>
[Config("Ant")]
public class AntSetting : Config<AntSetting>, IClientSetting
{
    #region 属性
    /// <summary>调试开关。默认false</summary>
    [Description("Debug switch. Defaults to false.")]
    public Boolean Debug { get; set; }

    /// <summary>调度中心。逗号分隔多地址，主备架构</summary>
    [Description("Scheduling center. Multiple addresses separated by commas, primary/secondary architecture.")]
    public String Server { get; set; } = "tcp://127.0.0.1:9999,tcp://ant.newlifex.com:9999";

    /// <summary>应用标识。调度中心以此隔离应用，默认当前应用</summary>
    [Description("Application identifier. The scheduling center uses it to isolate applications, defaults to the current application.")]
    public String AppId { get; set; }

    /// <summary>应用密钥。</summary>
    [Description("Application secret.")]
    public String Secret { get; set; }

    /// <summary>是否使用星尘注册中心解析服务地址。默认true；客户端经网关直连时建议关闭</summary>
    [Description("Whether to use the Stardust registry to resolve service addresses. Defaults to true; recommended to disable when the client connects directly through the gateway.")]
    public Boolean UseRegistry { get; set; } = true;

    /// <summary>消息型作业空闲申请周期（秒）。HTTP 通道收到通知会立即复位，默认15秒</summary>
    [Description("Idle request interval for message-type jobs (seconds). The HTTP channel resets it immediately upon receiving a notification, defaults to 15 seconds.")]
    public Int32 MessageIdlePeriod { get; set; } = 15;

    String IClientSetting.Code { get => AppId; set => AppId = value; }
    #endregion

    #region 方法
    /// <summary>重载</summary>
    protected override void OnLoaded()
    {
        if (AppId.IsNullOrEmpty())
        {
            var asm = Assembly.GetEntryAssembly();
            if (asm != null) AppId = asm.GetName().Name;
        }

        base.OnLoaded();
    }
    #endregion
}