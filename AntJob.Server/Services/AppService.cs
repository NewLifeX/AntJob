using System;
using System.Collections.Generic;
using System.Linq;
using AntJob.Data;
using AntJob.Data.Entity;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Net;
using NewLife.Security;
using AntJob.Models;

namespace AntJob.Server.Services;

public class AppService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;
    private readonly ILog _log;

    public AppService(ICacheProvider cacheProvider, ITracer tracer, ILog log)
    {
        _cacheProvider = cacheProvider;
        _tracer = tracer;
        _log = log;
    }

    #region 登录
    /// <summary>应用登录</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public (App, LoginResponse) Login(LoginModel model, String ip)
    {
        if (model.User.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.User));

        _log.Info("[{0}]从[{1}]登录[{2}@{3}]", model.User, ip, model.Machine, model.ProcessId);

        // 找应用
        var autoReg = false;
        var app = App.FindByName(model.User);
        if (app == null || app.Secret.MD5() != model.Pass)
        {
            app = CheckApp(app, model.User, model.Pass, ip);
            if (app == null) throw new ArgumentOutOfRangeException(nameof(model.User));

            autoReg = true;
        }

        if (app == null) throw new Exception($"应用[{model.User}]不存在！");
        if (!app.Enable) throw new Exception("已禁用！");

        // 核对密码
        if (!autoReg && !app.Secret.IsNullOrEmpty())
        {
            var pass2 = app.Secret.MD5();
            if (model.Pass != pass2) throw new Exception("密码错误！");
        }

        // 版本和编译时间
        if (app.Version.IsNullOrEmpty() || app.Version.CompareTo(model.Version) < 0) app.Version = model.Version;
        if (app.CompileTime < model.Compile) app.CompileTime = model.Compile;
        if (app.DisplayName.IsNullOrEmpty()) app.DisplayName = model.DisplayName;

        app.Save();

        // 应用上线
        var online = CreateOnline(app, _Net, model.Machine, model.ProcessId);
        online.Version = model.Version;
        online.CompileTime = model.Compile;
        online.Save();

        //// 记录当前用户
        //Session["App"] = app;

        WriteHistory(app, autoReg ? "注册" : "登录", true, $"[{model.User}/{model.Pass}]在[{model.Machine}@{model.ProcessId}]登录[{app}]成功");

        var rs = new LoginResponse { Name = app.Name, DisplayName = app.DisplayName };
        if (autoReg) rs.Secret = app.Secret;

        return (app, rs);
    }

    protected virtual App CheckApp(App app, String user, String pass, String ip)
    {
        //  本地账号不存在时
        var name = user;
        if (app == null)
        {
            // 是否支持自动注册
            var set = AntJobSetting.Current;
            if (!set.AutoRegistry) throw new Exception($"找不到应用[{name}]");

            app = new App
            {
                Secret = Rand.NextString(16)
            };
        }
        else if (app.Secret.MD5() != pass)
        {
            // 是否支持自动注册
            var set = AntJobSetting.Current;
            if (!set.AutoRegistry) throw new Exception($"应用[{name}]申请重新激活，但服务器设置禁止自动注册");

            if (app.Secret.IsNullOrEmpty()) app.Secret = Rand.NextString(16);
        }

        if (app.ID == 0)
        {
            app.Name = name;
            app.CreateIP = ip;
            app.CreateTime = DateTime.Now;

            // 首次打开
            app.Enable = true;
        }

        app.UpdateIP = ip;
        app.UpdateTime = DateTime.Now;

        //app.Save();

        return app;
    }
    #endregion

    #region 在线状态
    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    public PeerModel[] GetPeers(App app)
    {
        var olts = AppOnline.FindAllByAppID(app.ID);

        return olts.Select(e => e.ToModel()).ToArray();
    }

    AppOnline CreateOnline(App app, INetSession ns, String machine, Int32 pid)
    {
        var ip = ns.Remote.Host;

        var online = GetOnline(app, ns);
        online.Client = $"{(ip.IsNullOrEmpty() ? machine : ip)}@{pid}";
        online.Name = machine;
        online.ProcessId = pid;
        online.UpdateIP = ip;
        //online.Version = version;

        online.Server = Local + "";
        //online.Save();

        // 真正的用户
        Session["AppOnline"] = online;

        // 下线
        ns.OnDisposed += (s, e) =>
        {
            online.Delete();
            WriteHistory(online.App, "下线", true, $"[{online.Name}]登录于{online.CreateTime}，最后活跃于{online.UpdateTime}");
        };

        return online;
    }

    public AppOnline GetOnline(App app, INetSession ns)
    {
        if (Session["AppOnline"] is AppOnline online) return online;

        var ip = ns.Remote.Host;
        var ins = ns.Remote.EndPoint + "";
        online = AppOnline.FindByInstance(ins) ?? new AppOnline { CreateIP = ip };
        online.AppID = app.ID;
        online.Instance = ins;

        return online;
    }

    public void UpdateOnline(App app, JobTask ji, INetSession ns)
    {
        var online = GetOnline(app, ns);
        online.Total += ji.Total;
        online.Success += ji.Success;
        online.Error += ji.Error;
        online.Cost += ji.Cost;
        online.Speed = ji.Speed;
        online.LastKey = ji.Key;
        online.SaveAsync();
    }
    #endregion

    #region 写历史
    public void WriteHistory(App app, String action, Boolean success, String remark) => AppHistory.Create(app, action, success, remark, Local + "", _Net.Remote?.Host);
    #endregion
}
