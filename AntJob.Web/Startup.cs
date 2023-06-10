using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife.Cube;
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
    }
}