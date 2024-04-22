using AntJob;
using AntJob.Extensions;
using NewLife.Log;
using NewLife.Model;
using Stardust;

// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

var services = ObjectContainer.Current;
services.AddStardust();

var set = AntSetting.Current;

// 实例化调度器
var scheduler = new Scheduler
{
    ServiceProvider = services.BuildServiceProvider(),

    //// 使用分布式调度引擎替换默认的本地文件调度
    //Provider = new NetworkJobProvider
    //{
    //    Debug = set.Debug,
    //    Server = set.Server,
    //    AppID = set.AppID,
    //    Secret = set.Secret,
    //}
};

scheduler.Join(set.Server, set.AppID, set.Secret, set.Debug);

// 添加作业处理器
//sc.Handlers.Add(new CSharpHandler());
scheduler.AddHandler<SqlHandler>();
scheduler.AddHandler<SqlMessage>();

// 启动调度引擎，调度器内部多线程处理
scheduler.Start();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();
