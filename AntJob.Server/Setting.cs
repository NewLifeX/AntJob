using System.ComponentModel;
using NewLife;
using NewLife.Configuration;
using NewLife.Security;

namespace AntJob.Server;

/// <summary>配置</summary>
[Config("AntJob")]
public class AntJobSetting : Config<AntJobSetting>
{
    #region 属性
    /// <summary>调试开关。默认true</summary>
    [Description("Debug switch. Defaults to true.")]
    public Boolean Debug { get; set; } = true;

    /// <summary>端口</summary>
    [Description("Port")]
    public Int32 Port { get; set; } = 9999;

    /// <summary>令牌密钥。用于生成JWT令牌的算法和密钥，如HS256:ABCD1234</summary>
    [Description("Token secret. Algorithm and key used to generate JWT tokens, e.g. HS256:ABCD1234.")]
    public String TokenSecret { get; set; }

    /// <summary>令牌有效期。默认2*3600秒</summary>
    [Description("Token validity period. Defaults to 2*3600 seconds.")]
    public Int32 TokenExpire { get; set; } = 2 * 3600;

    /// <summary>会话超时。默认600秒</summary>
    [Description("Session timeout. Defaults to 600 seconds.")]
    public Int32 SessionTimeout { get; set; } = 600;

    /// <summary>自动注册。任意应用登录时自动注册，省去人工配置应用账号的麻烦，默认true</summary>
    [Description("Auto registration. Registers automatically when any application logs in, saving the trouble of manually configuring application accounts, defaults to true.")]
    public Boolean AutoRegistry { get; set; } = true;

    ///// <summary>Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁</summary>
    //[Description("Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁")]
    //public String RedisCache { get; set; }
    #endregion

    #region 方法
    /// <summary>加载后修正令牌密钥。未配置时自动生成，避免登录与心跳因空密钥失败</summary>
    protected override void OnLoaded()
    {
        if (TokenSecret.IsNullOrEmpty() || TokenSecret.Split(':').Length != 2)
            TokenSecret = $"HS256:{Rand.NextString(16)}";

        base.OnLoaded();
    }
    #endregion
}