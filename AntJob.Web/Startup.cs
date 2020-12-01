using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.WebMiddleware;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Web;
using Stardust.Monitors;
using XCode.DataAccessLayer;

namespace AntJob.Web
{
    public class Startup
    {
        public Startup() { }

        public void ConfigureServices(IServiceCollection services)
        {
            var set = Stardust.Setting.Current;
            if (!set.Server.IsNullOrEmpty())
            {
                // APM跟踪器
                var tracer = new StarTracer(set.Server) { Log = XTrace.Log };
                DefaultTracer.Instance = tracer;
                ApiHelper.Tracer = tracer;
                DAL.GlobalTracer = tracer;
                OAuthClient.Tracer = tracer;
                TracerMiddleware.Tracer = tracer;

                services.AddSingleton<ITracer>(tracer);
            }

            services.AddControllersWithViews();
            services.AddCube();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
        }
    }
}