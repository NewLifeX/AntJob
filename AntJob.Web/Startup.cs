using AntJob.Server;
using AntJob.Server.Services;
using NewLife.Cube;
using NewLife.Security;
using XCode;

namespace AntJob.Web;

public class Startup
{
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

        // 使用Cube前添加自己的管道
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/CubeHome/Error");

        //// 启用https
        //app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseCube(env);

        app.UseEndpoints(endpoints =>
        {
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