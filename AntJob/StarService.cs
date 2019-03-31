using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using NewLife.Log;
using NewLife.Net;
using NewLife.Remoting;
using Stardust.Data;

namespace Stardust
{
    [Api(null)]
    public class StarService : IApi, IActionFilter
    {
        #region 属性
        /// <summary>本地节点</summary>
        public static EndPoint Local { get; set; }
        #endregion

        #region 登录
        public IApiSession Session { get; set; }

        /// <summary>
        /// 传入应用名和密钥登陆，
        /// 返回应用名和应用显示名
        /// </summary>
        /// <param name="user">应用名</param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [Api(nameof(Login))]
        public Object Login(String user, String pass)
        {
            if (user.IsNullOrEmpty()) throw new ArgumentNullException(nameof(user));
            if (pass.IsNullOrEmpty()) throw new ArgumentNullException(nameof(pass));

            var ns = Session as INetSession;
            var ip = ns.Remote.Host;
            var ps = ControllerContext.Current.Parameters;

            WriteLog("[{0}]从[{1}]登录", user, ns.Remote);

            // 找应用
            var app = App.FindByName(user);
            if (app == null || app.Secret.IsNullOrEmpty())
            {
                if (app == null) app = new App();

                if (app.ID == 0)
                {
                    app.Name = user;
                    //app.Secret = pass;
                    app.CreateIP = ip;
                    app.CreateTime = DateTime.Now;
                    app.Enable = true;
                }

                var name = ps["name"] + "";
                if (!name.IsNullOrEmpty()) app.DisplayName = name;

                app.UpdateIP = ip;
                app.UpdateTime = DateTime.Now;

                app.Save();
            }

            if (!app.Enable) throw new Exception("已禁用！");

            // 核对密码
            if (!app.Secret.IsNullOrEmpty())
            {
                var pass2 = app.Secret.MD5();
                if (pass != pass2) throw new Exception("密码错误！");
            }

            // 应用上线
            CreateOnline(app, ns, ps);

            app.LastIP = ip;
            app.LastLogin = DateTime.Now;
            app.Save();

            // 记录当前用户
            Session["App"] = app;

            return new
            {
                app.Name,
                app.DisplayName,
            };
        }

        void IActionFilter.OnActionExecuting(ControllerContext filterContext)
        {
            var act = filterContext.ActionName;
            if (act == nameof(Login)) return;

            if (Session["App"] is App app)
            {
                var online = GetOnline(app, Session as INetSession);
                online.UpdateTime = DateTime.Now;
                online.SaveAsync();
            }
            else
            {
                var ns = Session as INetSession;
                throw new ApiException(401, "{0}未登录！不能执行{1}".F(ns.Remote, act));
            }
        }

        void IActionFilter.OnActionExecuted(ControllerContext filterContext)
        {
            var ex = filterContext.Exception;
            if (ex != null && !filterContext.ExceptionHandled)
            {
                // 显示错误
                if (ex is ApiException)
                    XTrace.Log.Error(ex.Message);
                else
                    XTrace.WriteException(ex);
            }
        }
        #endregion

        #region 业务
        /// <summary>报告服务列表</summary>
        /// <param name="services"></param>
        /// <returns></returns>
        [Api(nameof(Report))]
        public Boolean Report(String[] services)
        {
            return false;
        }
        #endregion

        #region 在线状态
        AppOnline CreateOnline(IApp app, INetSession ns, IDictionary<String, Object> ps)
        {
            var ip = ns.Remote.Host;

            var machine = ps["machine"] + "";
            var pid = ps["processid"].ToInt();
            var ver = ps["version"] + "";
            var compile = ps["compile"].ToDateTime();

            var online = GetOnline(app, ns);

            // 客户端特性
            online.Client = $"{(ip.IsNullOrEmpty() ? machine : ip)}@{pid}";
            online.Name = machine;
            online.Version = ver;
            online.Compile = compile;

            // 服务器特性
            pid = Process.GetCurrentProcess().Id;
            online.Server = Local + "@" + pid;
            online.Save();

            // 真正的用户
            Session["AppOnline"] = online;

            // 下线
            ns.OnDisposed += (s, e) => online.Delete();

            // 版本和编译时间
            if (app.Version.IsNullOrEmpty() || app.Version.CompareTo(ver) < 0) app.Version = ver;
            if (app.Compile.Year < 2000 || app.Compile < compile) app.Compile = compile;

            return online;
        }

        AppOnline GetOnline(IApp app, INetSession ns)
        {
            if (Session["AppOnline"] is AppOnline online) return online;

            var ip = ns.Remote.Host;
            var ins = ns.Remote.EndPoint + "";
            online = AppOnline.FindBySession(ins) ?? new AppOnline { CreateIP = ip };
            online.AppID = app.ID;
            online.Session = ins;

            return online;
        }
        #endregion

        #region 日志
        /// <summary>日志</summary>
        public static ILog Log { get; set; }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
        #endregion
    }
}