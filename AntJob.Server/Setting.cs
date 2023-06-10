using System.ComponentModel;
using NewLife.Configuration;

namespace AntJob.Server;

/// <summary>配置</summary>
[Config("AntJob")]
public class Setting : Config<Setting>
{
    #region 属性
    /// <summary>调试开关。默认true</summary>
    [Description("调试开关。默认true")]
    public Boolean Debug { get; set; } = true;

    /// <summary>端口</summary>
    [Description("端口")]
    public Int32 Port { get; set; } = 9999;

    /// <summary>自动注册。任意应用登录时自动注册，省去人工配置应用账号的麻烦，默认true</summary>
    [Description("自动注册。任意应用登录时自动注册，省去人工配置应用账号的麻烦，默认true")]
    public Boolean AutoRegistry { get; set; } = true;

    /// <summary>Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁</summary>
    [Description("Redis缓存。设置用于控制任务切分的分布式锁，默认为空使用本进程内存锁")]
    public String RedisCache { get; set; }
    #endregion
}