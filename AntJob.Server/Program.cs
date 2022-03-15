using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using AntJob.Data.Entity;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Threading;
using Stardust;

namespace AntJob.Server
{
    class Program
    {
        static void Main(String[] args)
        {
            // 启用控制台日志，拦截所有异常
            XTrace.UseConsole();

            // 配置星尘。自动读取配置文件 config/star.config 中的服务器地址、应用标识、密钥
            //var star = new StarFactory(null, null, null);
            //if (star.Server.IsNullOrEmpty()) star = null;

            var set = Setting.Current;

            var server = new ApiServer(set.Port)
            {
                //Tracer = star?.Tracer,
                ShowError = true,
                Log = XTrace.Log,
            };

            server.Register<AntService>();

            AntService.Log = XTrace.Log;

            // 本地结点
            AntService.Local = new IPEndPoint(NetHelper.MyIP(), set.Port);

            // 数据缓存，也用于全局锁，支持MemoryCache和Redis
            if (!set.RedisCache.IsNullOrEmpty())
            {
                var redis = new Redis { Timeout = 5_000 + 1_000 };
                redis.Init(set.RedisCache);
                AntService.Cache = redis;
            }
            else
            {
                AntService.Cache = new MemoryCache();
            }

            server.Start();

            _clearOnlineTimer = new TimerX(ClearOnline, null, 1000, 10 * 1000);
            _clearItemTimer = new TimerX(ClearItems, null, 10_000, 3600_000) { Async = true };

            // 友好退出
            //ObjectContainer.Current.BuildHost().Run();u
            Thread.Sleep(-1);
        }

        static void InitData()
        {
            var n = App.Meta.Count;

            var set = NewLife.Setting.Current;
            if (set.IsNew)
            {
                set.DataPath = @"..\Data";

                set.Save();
            }

            var set2 = XCode.Setting.Current;
            if (set2.IsNew)
            {
                set2.Debug = true;
                set2.ShowSQL = false;
                set2.TraceSQLTime = 3000;
                //set2.SQLiteDbPath = @"..\Data";

                set2.Save();
            }
        }

        #region 清理过时
        private static TimerX _clearOnlineTimer;

        //每10s清除一次UpdateTime超10分钟未更新的
        private static void ClearOnline(Object state)
        {
            var ls = AppOnline.GetOnlines(10);
            foreach (var item in ls)
            {
                item.Delete();
            }
        }
        #endregion

        #region 清理任务项
        private static TimerX _clearItemTimer;

        private static void ClearItems(Object state)
        {
            // 遍历所有作业
            var p = 0;
            var rs = 0;
            var sw = Stopwatch.StartNew();
            while (true)
            {
                var list = Job.FindAll(null, null, null, p, 1000);
                if (list.Count == 0) break;

                foreach (var job in list)
                {
                    try
                    {
                        rs += job.DeleteItems();
                    }
                    catch (Exception ex)
                    {
                        XTrace.WriteException(ex);
                    }
                }

                if (list.Count < 1000) break;
                p += list.Count;
            }

            if (rs > 0)
            {
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                var speed = rs * 1000 / ms;
                XTrace.WriteLine("共删除作业项[{0:n0}]行，耗时{1:n0}ms，速度{2:n0}tps", rs, ms, speed);
            }
        }
        #endregion
    }
}