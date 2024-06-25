using AntJob.Server;
using AntJob.Server.Services;
using NewLife.Caching;
using NewLife.Caching.Services;
using NewLife.Log;
using NewLife.Model;
using NewLife.Security;
using NewLife.Serialization;
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

//// 过渡期暂时使用FastJson，为了兼容旧数据序列化Start
//JsonHelper.Default = new FastJson();

services.AddSingleton(AntJobSetting.Current);

// 分布式缓存，锚定配置中心RedisCache，若无配置则使用本地MemoryCache
// 集群部署时，务必使用RedisCache，内部将使用Redis实现分布式锁
services.AddSingleton<ICacheProvider, RedisCacheProvider>();
services.AddSingleton<AppService>();
services.AddSingleton<JobService>();

// 注册密码提供者，用于通信过程中保护密钥，避免明文传输
services.AddSingleton<IPasswordProvider>(new SaltPasswordProvider { Algorithm = "md5", SaltTime = 60 });

// 预热数据层，执行反向工程建表等操作
EntityFactory.InitConnection("Ant");

// 修正旧版数据
_ = Task.Run(() => JobService.FixOld());

// 友好退出
var host = services.BuildHost();

host.Add<Worker>();

await host.RunAsync();