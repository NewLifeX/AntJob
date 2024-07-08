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
    Log = XTrace.Log,
};

scheduler.Join(set);

// 添加作业处理器
//sc.Handlers.Add(new CSharpHandler());
scheduler.AddHandler<SqlHandler>();
scheduler.AddHandler<SqlMessage>();

// 启动调度引擎，调度器内部多线程处理
scheduler.StartAsync();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();
