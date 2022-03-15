using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NewLife.Cube;
using NewLife.Log;

namespace AntJob.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XTrace.UseConsole();

            var app = ApplicationManager.Load();
            do
            {
                app.Start(CreateHostBuilder(args).Build());
            } while (app.Restarting);
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
}