﻿using System;
using System.Threading;
using AntJob.Providers;
using NewLife.Log;
using NewLife.Model;
using Stardust;

namespace AntJob.Agent;

class Program
{
    static void Main(String[] args)
    {
        // 启用控制台日志，拦截所有异常
        XTrace.UseConsole();

        var services = ObjectContainer.Current;
        var star = new StarFactory();

        var set = AntSetting.Current;

        // 实例化调度器
        var scheduler = new Scheduler
        {
            ServiceProvider = services.BuildServiceProvider(),

            // 使用分布式调度引擎替换默认的本地文件调度
            Provider = new NetworkJobProvider
            {
                Debug = set.Debug,
                Server = set.Server,
                AppID = set.AppID,
                Secret = set.Secret,
            }
        };

        // 添加作业处理器
        //sc.Handlers.Add(new CSharpHandler());
        scheduler.AddHandler<SqlHandler>();
        scheduler.AddHandler<SqlMessage>();

        // 启动调度引擎，调度器内部多线程处理
        scheduler.Start();

        // 友好退出
        //ObjectContainer.Current.BuildHost().Run();
        Thread.Sleep(-1);
    }
}