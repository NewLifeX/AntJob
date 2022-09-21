using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewLife.Log;

namespace HisAgent;

class Program
{
    static void Main(string[] args)
    {
        XTrace.UseConsole();

        CreateHostBuilder(args).Build().Run();
    }

    /// <summary></summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) => ConfigureServices(services));

    /// <summary></summary>
    /// <param name="hostBuilderContext"></param>
    /// <param name="services"></param>
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddStardust();

        // 添加后台调度服务
        services.AddHostedService<JobHost>();
    }
}