using System;
using System.Net;

namespace Stardust
{
    class Program
    {
        static void Main(String[] args) => new MyService().Main();
    }

    /// <summary>服务类。名字可以自定义</summary>
    class MyService : AgentServiceBase<MyService>
    {
        /// <summary>是否使用线程池调度。false表示禁用线程池，改用Agent线程</summary>
        public Boolean Pooling { get; set; } = true;

        public MyService()
        {
            ServiceName = "Stardust";

            ThreadPoolX.QueueUserWorkItem(() =>
            {
                var n = App.Meta.Count;
                AppStat.Meta.Session.Dal.Db.ShowSQL = false;

                var set2 = XCode.Setting.Current;
                if (set2.IsNew)
                {
                    set2.Debug = true;
                    set2.ShowSQL = false;
                    set2.TraceSQLTime = 3000;
                    set2.SQLiteDbPath = @"..\Data";

                    set2.Save();
                }
            });

            // 注册菜单，在控制台菜单中按 t 可以执行Test函数，主要用于临时处理数据
            AddMenu('t', "数据测试", Test);
        }

        ApiServer _Server;
        private void Init()
        {
            var sc = _Server;
            if (sc == null)
            {
                var set = Setting.Current;

                sc = new ApiServer(set.Port)
                {
                    Log = XTrace.Log
                };
                if (set.Debug)
                {
                    var ns = sc.EnsureCreate() as NetServer;
                    ns.Log = XTrace.Log;
#if DEBUG
                    ns.LogSend = true;
                    ns.LogReceive = true;
                    sc.EncoderLog = XTrace.Log;
#endif
                }

                // 注册服务
                sc.Register<StarService>();

                StarService.Log = XTrace.Log;
                StarService.Local = new IPEndPoint(NetHelper.MyIP(), set.Port);

                sc.Start();

                _Server = sc;
            }
        }

        /// <summary>服务启动</summary>
        /// <remarks>
        /// 安装Windows服务后，服务启动会执行一次该方法。
        /// 控制台菜单按5进入循环调试也会执行该方法。
        /// </remarks>
        protected override void StartWork(String reason)
        {
            Init();

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

            _Server.TryDispose();
            _Server = null;
        }

        /// <summary>数据测试，菜单t</summary>
        public void Test()
        {
        }
    }
}