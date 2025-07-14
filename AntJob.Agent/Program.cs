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
services.AddSingleton(set);

// 实例化调度器
services.AddAntJob()
    .AddHandler<SqlHandler>()
    .AddHandler<SqlMessage>();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();
