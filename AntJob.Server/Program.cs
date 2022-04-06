using AntJob.Server;
using NewLife.Log;


// 启用控制台日志，拦截所有异常
XTrace.UseConsole();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // 配置星尘。借助StarAgent，或者读取配置文件 config/star.config 中的服务器地址
        var star = services.AddStardust(null);

        // 注册后台服务
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();