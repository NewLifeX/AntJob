using System.Reflection;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using NewLife;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Remoting.Models;
using NewLife.Security;
using NewLife.Web;

namespace AntJob.Server.Services;

public class AppService
{
    private readonly IPasswordProvider _passwordProvider;
    private readonly AntJobSetting _setting;
    private readonly ILog _log;

    public AppService(IPasswordProvider passwordProvider, AntJobSetting setting, ILog log)
    {
        _passwordProvider = passwordProvider;
        _setting = setting;
        _log = log;
    }

    #region 登录
    /// <summary>应用登录</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public (App, AppOnline, LoginResponse) Login(LoginModel model, String sessionId, String ip)
    {
        if (model.Code.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.Code));

        _log.Info("[{0}]从[{1}]登录[{2}]", model.Code, ip, model.ClientId);

        // 找应用
        var autoReg = false;
        var app = App.FindByName(model.Code);

        // 登录密码未设置或者未提交，则执行动态注册
        var secret = model.Secret;
        if (app == null || !app.Secret.IsNullOrEmpty() && secret.IsNullOrEmpty())
        {
            app = CheckApp(app, model.Code, model.Secret, ip);
            if (app == null) throw new ArgumentOutOfRangeException(nameof(model.Code));

            autoReg = true;
        }

        if (app == null) throw new Exception($"应用[{model.Code}]不存在！");
        if (!app.Enable) throw new Exception("已禁用！");

        // 核对密码
        if (!autoReg && !app.Secret.IsNullOrEmpty())
        {
            if (secret.IsNullOrEmpty() || !_passwordProvider.Verify(app.Secret, secret)) throw new Exception("密码错误！");
        }

        // 版本和编译时间。应用实体由单对象缓存共享，多实例并发登录时需串行写入
        var compile = model.Compile.ToDateTime().ToLocalTime();
        try
        {
            lock (app)
            {
                if (app.Version.IsNullOrEmpty() || app.Version.CompareTo(model.Version) < 0) app.Version = model.Version;
                if (app.CompileTime < compile) app.CompileTime = compile;
                if (app.DisplayName.IsNullOrEmpty()) app.DisplayName = model.DisplayName;

                // 多客户端并发自动注册时，可能同时插入同名应用而触发唯一约束，冲突后重新加载
                app.Save();
            }
        }
        catch (Exception) when (autoReg)
        {
            var app2 = App.FindByName(model.Code);
            if (app2 == null) throw;

            app = app2;
        }

        // 应用上线。RPC用ClientId，Web用AppName@IP
        //if (!model.ClientId.IsNullOrEmpty()) sessionId = model.ClientId;
        if (sessionId.IsNullOrEmpty()) sessionId = model.ClientId;
        var online = GetOnline(app, sessionId, ip);

        void FillOnline(AppOnline entity)
        {
            entity.Name = model.Machine;
            entity.Client = model.ClientId;
            entity.ProcessId = model.ProcessId;
            entity.Version = model.Version;
            entity.CompileTime = compile;
        }

        FillOnline(online);
        // 在线实体被同一实例的并发心跳、申请与上报共用，串行化写入避免脏标记互相覆盖
        try
        {
            lock (online)
            {
                online.Save();
            }
        }
        catch (Exception) when (online.ID == 0)
        {
            // 同一实例并发登录时可能同时插入而触发唯一约束，冲突后重新加载已有记录
            var entity = AppOnline.Find(AppOnline._.Instance == online.Instance);
            if (entity == null) throw;

            online = entity;
            FillOnline(online);
            lock (online)
            {
                online.Save();
            }
        }

        WriteHistory(app, autoReg ? "注册" : "登录", true, $"[{model.Code}/{model.Secret}]在[{model.ClientId}]登录[{app}]成功", ip);

        var rs = new LoginResponse { Name = app.Name };
        if (autoReg) rs.Secret = app.Secret;

        return (app, online, rs);
    }

    protected virtual App CheckApp(App app, String user, String pass, String ip)
    {
        //  本地账号不存在时
        var name = user;
        if (app == null)
        {
            // 是否支持自动注册
            //var set = AntJobSetting.Current;
            if (!_setting.AutoRegistry) throw new Exception($"找不到应用[{name}]");

            app = new App
            {
                //Secret = Rand.NextString(16)
            };
        }
        else if (app.Secret.MD5() != pass)
        {
            // 是否支持自动注册
            //var set = AntJobSetting.Current;
            if (!_setting.AutoRegistry) throw new Exception($"应用[{name}]申请重新激活，但服务器设置禁止自动注册");

            //if (app.Secret.IsNullOrEmpty()) app.Secret = Rand.NextString(16);
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

    public ILogoutResponse Logout(App app, AppOnline online, String reason, String ip)
    {
        if (app != null)
        {
            //online ??= GetOnline(app, ip);
            if (online != null)
            {
                WriteHistory(app, "注销", true, reason, ip);

                online.Delete();
            }
        }

        return new LogoutResponse { Name = app?.Name };
    }
    #endregion

    #region 在线状态
    /// <summary>构建在线会话标识。优先使用客户端标识，未提供时退化为应用@IP，并限制在字段长度内</summary>
    /// <param name="app">应用名</param>
    /// <param name="clientId">客户端标识</param>
    /// <param name="ip">客户端地址</param>
    /// <returns></returns>
    public static String BuildSessionId(String app, String clientId, String ip)
    {
        var id = clientId.IsNullOrEmpty() ? $"{app}@{ip}" : $"{app}@{clientId}";

        return id.Length > 50 ? id[..50] : id;
    }

    public IPingResponse Ping(App app, AppOnline online, IPingRequest request, String ip)
    {
        if (app != null)
        {
            //online ??= GetOnline(app, ip);
            if (online != null)
            {
                if (request is PingRequest req)
                {
                }

                // 同一在线实例被并发上报共用，串行化实体更新，避免计数被覆盖
                lock (online)
                {
                    online.UpdateIP = ip;
                    // 显式刷新更新时间，保证心跳能延长在线记录寿命
                    online.UpdateTime = DateTime.Now;
                    online.Save();
                }
            }
        }

        return new PingResponse
        {
            Time = request.Time,
            ServerTime = DateTime.UtcNow.ToLong(),
        };
    }

    /// <summary>清理长时间未更新的在线记录</summary>
    /// <param name="timeout">超时秒数，默认600秒（对应 AntJobSetting.SessionTimeout）</param>
    /// <returns>清理条数</returns>
    public static Int32 ClearOnline(Int32 timeout = 600)
    {
        // 条件删除由数据库在同一条语句内完成，避免查询后心跳刷新了记录仍被删除
        return AppOnline.Delete(AppOnline._.UpdateTime <= DateTime.Now.AddSeconds(-timeout));
    }

    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    public PeerModel[] GetPeers(App app)
    {
        var olts = AppOnline.FindAllByAppID(app.ID);

        return olts.Select(e => e.ToModel()).ToArray();
    }

    public AppOnline GetOnline(App app, String sessionId, String ip)
    {
        if (sessionId.IsNullOrEmpty()) sessionId = $"{app.Name}@{ip}";

        var online = AppOnline.GetOrAdd(sessionId);
        // 在线实体被同实例的并发请求共用，属性写入需要串行
        lock (online)
        {
            online.AppID = app.ID;
            online.Server = Environment.MachineName;

            if (online.CreateIP.IsNullOrEmpty()) online.CreateIP = ip;
            online.UpdateIP = ip;
        }

        return online;
    }

    public void UpdateOnline(App app, JobTask ji, AppOnline online)
    {
        // 同一在线实例被并发上报共用，串行化实体更新，避免计数被覆盖
        lock (online)
        {
            online.Total += ji.Total;
            online.Success += ji.Success;
            //online.Error += ji.Error;
            online.Cost += ji.Cost;
            online.Speed = ji.Speed;
            online.LastKey = ji.Key;

            if (ji.Status == JobStatus.错误) online.Error++;

            // 显式刷新更新时间，保证上报能延长在线记录寿命
            online.UpdateTime = DateTime.Now;
            online.Update();
        }
    }
    #endregion

    #region 写历史
    public void WriteHistory(App app, String action, Boolean success, String remark, String ip = null) =>
        AppHistory.Create(app, action, success, remark, Environment.MachineName, ip);
    #endregion

    #region 辅助
    public TokenModel IssueToken(String name, AntJobSetting set)
    {
        // 颁发令牌
        var ss = set.TokenSecret.Split(':');
        var jwt = new JwtBuilder
        {
            Issuer = Assembly.GetEntryAssembly().GetName().Name,
            Subject = name,
            Id = Rand.NextString(8),
            Expire = DateTime.Now.AddSeconds(set.TokenExpire),

            Algorithm = ss[0],
            Secret = ss[1],
        };

        return new TokenModel
        {
            AccessToken = jwt.Encode(null),
            TokenType = jwt.Type ?? "JWT",
            ExpireIn = set.TokenExpire,
            RefreshToken = jwt.Encode(null),
        };
    }

    public (App, Exception) DecodeToken(String token, String tokenSecret)
    {
        if (token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(token));
        //if (token.IsNullOrEmpty()) throw new ApiException(401, $"节点未登录[ip={UserHost}]");

        // 解码令牌
        var ss = tokenSecret.Split(':');
        var jwt = new JwtBuilder
        {
            Algorithm = ss[0],
            Secret = ss[1],
        };

        var rs = jwt.TryDecode(token, out var message);
        var app = App.FindByName(jwt.Subject);

        Exception ex = null;
        if (!rs || app == null)
        {
            if (app != null)
                ex = new ApiException(403, $"[{app.Name}/{app.DisplayName}]非法访问 {message}");
            else
                ex = new ApiException(403, $"[{jwt.Subject}]非法访问 {message}");
        }

        return (app, ex);
    }

    public TokenModel ValidAndIssueToken(String deviceCode, String token, AntJobSetting set)
    {
        if (token.IsNullOrEmpty()) return null;
        //var set = Setting.Current;

        // 令牌有效期检查，10分钟内过期者，重新颁发令牌
        var ss = set.TokenSecret.Split(':');
        var jwt = new JwtBuilder
        {
            Algorithm = ss[0],
            Secret = ss[1],
        };
        var rs = jwt.TryDecode(token, out var message);
        return !rs || jwt == null ? null : DateTime.Now.AddMinutes(10) > jwt.Expire ? IssueToken(deviceCode, set) : null;
    }
    #endregion
}
