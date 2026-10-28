using AntJob;
using HisAgent;
using NewLife.Log;
using NewLife.Model;
using Stardust;
using XCode;

// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

// 示例默认关闭 SQL 日志，避免日志被 SQL 淹没；需要排查时改为 true
var xset = XCodeSetting.Current;
if (xset.IsNew)
{
    xset.ShowSQL = false;
    xset.Save();
}

// 预热数据层，先完成建表再启动作业，避免首个任务与反向工程竞争
EntityFactory.InitConnection("His");

var services = ObjectContainer.Current;
services.AddStardust();

services.AddAntJob()
    .AddHandler<HelloJob>()
    .AddHandler<BuildPatient>()
    .AddHandler<BuildWill>();

// 友好退出
var host = services.BuildHost();
await host.RunAsync();