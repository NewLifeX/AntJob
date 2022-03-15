using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife.Cube;
using Stardust;

namespace AntJob.Web
{
    public class Startup
    {
        public Startup() { }

        public void ConfigureServices(IServiceCollection services)
        {
            #region 星尘配置内容
            //var star = new StarFactory(null, null, null);

            //services.AddSingleton(star);
            //services.AddSingleton(star.Tracer);
            //services.AddSingleton(star.Config);
            #endregion

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