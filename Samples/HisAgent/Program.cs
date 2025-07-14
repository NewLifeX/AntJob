using AntJob;
using HisAgent;
using NewLife.Log;
using NewLife.Model;
using Stardust;

// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

var services = ObjectContainer.Current;
services.AddStardust();

services.AddSingleton(AntSetting.Current);

services.AddAntJob()
    .AddHandler<HelloJob>()
    .AddHandler<BuildPatient>()
    .AddHandler<BuildWill>();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();