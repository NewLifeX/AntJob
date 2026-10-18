using AntJob.Server;
using AntJob.Server.Services;
using AntJob.Web.Services;
using NewLife;
using NewLife.Cube;
using NewLife.Security;
using NewLife.Threading;
using XCode;

namespace AntJob.Web;

public class Startup
{
    private TimerX _clearOnlineTimer;
    private TimerX _clearItemsTimer;

    public Startup() { }

    public void ConfigureServices(IServiceCollection services)
    {
        // 配置星尘。借助StarAgent，或者读取配置文件 config/star.config 中的服务器地址
        var star = services.AddStardust(null);

        // 默认数据目录
        var set = NewLife.Setting.Current;
        if (set.IsNew)
        {
            set.DataPath = "../Data";
            set.BackupPath = "../Backup";
            set.Save();
        }

        services.AddSingleton(AntJobSetting.Current);

        services.AddSingleton<AppService>();
        services.AddSingleton<JobService>();

        // 作业变更与消息到达通知，经事件总线分发到本机 WebSocket 会话
        services.AddSingleton<IJobNotifier, EventBusNotifier>();
        services.AddSingleton<AntNotifyManager>();

        // 注册密码提供者，用于通信过程中保护密钥，避免明文传输
        services.AddSingleton<IPasswordProvider>(new SaltPasswordProvider { Algorithm = "md5", SaltTime = 60 });

        services.AddControllersWithViews();
        services.AddCube();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 预热数据层，执行反向工程建表等操作
        EntityFactory.InitConnection("Membership");
        EntityFactory.InitConnection("Log");
        EntityFactory.InitConnection("Cube");
        EntityFactory.InitConnection("Ant");

        // 修正旧版数据
        Task.Run(() => JobService.FixOld());

        // 定时清理长时间未心跳的在线记录。HTTP 客户端异常退出时不会主动注销
        var sessionTimeout = AntJobSetting.Current.SessionTimeout;
        _clearOnlineTimer = new TimerX(s => AppService.ClearOnline(sessionTimeout), null, 10_000, 10_000) { Async = true };

        // 定时清理过期任务。纯 Web 部署没有 AntServer 的 Worker 承担该工作
        _clearItemsTimer = new TimerX(s => JobService.ClearItems(), null, 60_000, 3600_000) { Async = true };

        // 停机时释放定时器，避免关停过程中继续写库
        var lifetime = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>(app.ApplicationServices);
        lifetime?.ApplicationStopped.Register(() =>
        {
            _clearOnlineTimer.TryDispose();
            _clearItemsTimer.TryDispose();
        });

        // 使用Cube前添加自己的管道
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/CubeHome/Error");

        //// 启用https
        //app.UseHttpsRedirection();

        app.UseStaticFiles();

        // 客户端下行通知使用 WebSocket 长连接
        app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(60) });

        // 提前实例化通知器，确保启动时即订阅作业变更事件
        app.ApplicationServices.GetService<IJobNotifier>();

        app.UseCube(env);

        app.UseEndpoints(endpoints =>
        {
            // AntJobController 使用 [Route("AntJob")] 特性路由，必须显式映射
            endpoints.MapControllers();

            endpoints.MapControllerRoute(
                "Default",
                "{controller=CubeHome}/{action=Index}/{id?}"
                );
        });

        // 启用星尘注册中心，向注册中心注册服务，服务消费者将自动更新服务端地址列表
        app.RegisterService("AntWeb", null, env.EnvironmentName);
        //app.RegisterService("AntServer", null, env.EnvironmentName);
    }
}