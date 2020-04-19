using System;
using System.Diagnostics;
using System.Net;
using AntJob.Data.Entity;
using NewLife;
using NewLife.Agent;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Threading;

namespace AntJob.Server
{
    class Program
    {
        static void Main(String[] args) => new MyService().Main(args);
    }

    /// <summary>服务类。名字可以自定义</summary>
    class MyService : ServiceBase
    {
        public MyService()
        {
            ServiceName = "AntServer";

            ThreadPoolX.QueueUserWorkItem(() =>
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
            });

            // 注册菜单，在控制台菜单中按 t 可以执行Test函数，主要用于临时处理数据
            AddMenu('t', "数据测试", Test);
        }

        private ApiServer _server;
        /// <summary>服务启动</summary>
        /// <remarks>
        /// 安装Windows服务后，服务启动会执行一次该方法。
        /// 控制台菜单按5进入循环调试也会执行该方法。
        /// </remarks>
        protected override void StartWork(String reason)
        {
            var set = Setting.Current;

            var svr = new ApiServer(set.Port)
            {
                ShowError = true,
                Log = XTrace.Log,
            };

            //var ts = new AntService();
            //svr.Register(ts, null);
            svr.Register<AntService>();

            AntService.Log = XTrace.Log;

            // 本地结点
            AntService.Local = new IPEndPoint(NetHelper.MyIP(), set.Port);

            svr.Start();

            _server = svr;

            _clearOnlineTimer = new TimerX(ClearOnline, null, 1000, 10 * 1000);
            _clearItemTimer = new TimerX(ClearItems, null, 10_000, 3600_000) { Async = true };

            base.StartWork(reason);
        }

        /// <summary>服务停止</summary>
        /// <remarks>
        /// 安装Windows服务后，服务停止会执行该方法。
        /// 控制台菜单按5进入循环调试，任意键结束时也会执行该方法。
        /// </remarks>
        protected override void StopWork(String reason)
        {
            base.StopWork(reason);

            _server.TryDispose();
            _server = null;

            _clearOnlineTimer.TryDispose();
            _clearOnlineTimer = null;

            _clearItemTimer.TryDispose();
            _clearItemTimer = null;
        }

        /// <summary>数据测试，菜单t，默认文件调度</summary>
        public void Test()
        {
            Job.Meta.Session.Dal.Db.ShowSQL = true;

            ClearItems(null);
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