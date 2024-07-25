using NewLife.Cube;
using NewLife.Log;

namespace AntJob.Web;

public class Program
{
    public static void Main(string[] args)
    {
        XTrace.UseConsole();

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(String[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging => { logging.AddXLog(); })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //webBuilder.UseUrls("http://*:5000;https://*:5001");
                webBuilder.UseStartup<Startup>();
            });
    }
}