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

// 友好退出
var host = services.BuildHost();

host.Add<JobHost>();

await host.RunAsync();