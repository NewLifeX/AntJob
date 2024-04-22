using System.ComponentModel;
using NewLife.Configuration;

namespace AntJob.Server;

/// <summary>配置</summary>
[Config("AntJob")]
public class AntJobSetting : Config<AntJobSetting>
{
    #region 属性
    /// <summary>调试开关。默认true</summary>
    [Description("调试开关。默认true")]
    public Boolean Debug { get; set; } = true;

    /// <summary>端口</summary>
    [Description("端口")]
    public Int32 Port { get; set; } = 9999;

    /// <summary>令牌密钥。用于生成JWT令牌的算法和密钥，如HS256:ABCD1234</summary>
    [Description("令牌密钥。用于生成JWT令牌的算法和密钥，如HS256:ABCD1234")]
    public String TokenSecret { get; set; }

    /// <summary>令牌有效期。默认2*3600秒</summary>
    [Description("令牌有效期。默认2*3600秒")]
    public Int32 TokenExpire { get; set; } = 2 * 3600;

    /// <summary>会话超时。默认600秒</summary>
    [Description("会话超时。默认600秒")]
    public Int32 SessionTimeout { get; set; } = 600;

    /// <summary>自动注册。任意应用登录时自动注册，省去人工配置应用账号的麻烦，默认true</summary>
    [Description("自动注册。任意应用登录时自动注册，省去人工配置应用账号的麻烦，默认true")]
    public Boolean AutoRegistry { get; set; } = true;

    ///// <summary>Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁</summary>
    //[Description("Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁")]
    //public String RedisCache { get; set; }
    #endregion
}