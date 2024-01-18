using AntJob.Server;
using AntJob.Server.Services;
using NewLife.Caching;
using NewLife.Caching.Services;
using NewLife.Log;
using NewLife.Model;
using Stardust;
using XCode;

// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

var services = ObjectContainer.Current;
services.AddStardust();

// 默认数据目录
var set = NewLife.Setting.Current;
if (set.IsNew)
{
    set.DataPath = "../Data";
    set.BackupPath = "../Backup";
    set.Save();
}
var set2 = XCodeSetting.Current;
if (set2.IsNew)
{
    set2.ShowSQL = false;
    set2.Save();
}

services.AddSingleton(AntJobSetting.Current);

// 分布式缓存，锚定配置中心RedisCache，若无配置则使用本地MemoryCache
// 集群部署时，务必使用RedisCache，内部将使用Redis实现分布式锁
services.AddSingleton<ICacheProvider, RedisCacheProvider>();
services.AddSingleton<AppService>();
services.AddSingleton<JobService>();

// 预热数据层，执行反向工程建表等操作
EntityFactory.InitConnection("Ant");

// 友好退出
var host = services.BuildHost();

host.Add<Worker>();

await host.RunAsync();