using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NewLife.Log;

namespace HisWeb;

public class Program
{
    public static void Main(string[] args)
    {
        XTrace.UseConsole();

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
