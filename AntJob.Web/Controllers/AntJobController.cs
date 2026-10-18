using System.Reflection;
using System.Text.Json;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server;
using AntJob.Server.Services;
using AntJob.Web.Models;
using AntJob.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife;
using NewLife.Cube;
using NewLife.Data;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Remoting.Extensions.Services;
using NewLife.Remoting.Models;
using NewLife.Security;
using NewLife.Serialization;
using NewLife.Web;
using ApiFilterAttribute = NewLife.Remoting.Extensions.ApiFilterAttribute;
using IActionFilter = Microsoft.AspNetCore.Mvc.Filters.IActionFilter;

namespace AntJob.Web.Controllers;

[ApiController]
[Route("[controller]")]
[ApiFilter]
public class AntJobController : ControllerBase, IActionFilter
{
    ///// <summary>令牌</summary>
    //public String Token { get; private set; }

    /// <summary>用户主机</summary>
    public String UserHost => HttpContext.GetUserHost();

    /// <summary>构建在线会话标识。优先使用客户端标识，未提供时退化为应用@IP，并限制在字段长度内</summary>
    /// <param name="app">应用名</param>
    /// <param name="clientId">客户端标识</param>
    /// <param name="ip">客户端地址</param>
    /// <returns></returns>
    private static String BuildSessionId(String app, String clientId, String ip)
    {
        var id = clientId.IsNullOrEmpty() ? $"{app}@{ip}" : $"{app}@{clientId}";

        return id.Length > 50 ? id[..50] : id;
    }

    private App _App;
    private IDictionary<String, Object> _args;
    private AntJobSetting _setting;
    private readonly AppService _appService;
    private readonly JobService _jobService;
    private readonly AntNotifyManager _notifyManager;

    #region 构造
    public AntJobController(AppService appService, JobService jobService, AntJobSetting setting, AntNotifyManager notifyManager)
    {
        _appService = appService;
        _jobService = jobService;
        _setting = setting;
        _notifyManager = notifyManager;
    }

    void IActionFilter.OnActionExecuting(ActionExecutingContext context)
    {
        _args = context.ActionArguments;

        var token = ApiFilterAttribute.GetToken(context.HttpContext);

        try
        {
            if (context.ActionDescriptor is ControllerActionDescriptor act && !act.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)))
            {
                var rs = !token.IsNullOrEmpty() && OnAuthorize(token);
                if (!rs) throw new ApiException(403, "认证失败");
            }
        }
        catch (Exception ex)
        {
            var traceId = DefaultSpan.Current?.TraceId;
            context.Result = ex is ApiException aex
                ? new JsonResult(new { code = aex.Code, data = aex.Message, traceId })
                : new JsonResult(new { code = 500, data = ex.Message, traceId });

            WriteError(ex, context);
        }
    }

    void IActionFilter.OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null) WriteError(context.Exception, context);
    }

    protected Boolean OnAuthorize(String token)
    {
        var (app, ex) = _appService.DecodeToken(token, _setting.TokenSecret);
        _App = app;
        if (ex != null) throw ex;

        return app != null;
    }

    private void WriteError(Exception ex, ActionContext context)
    {
        // 拦截全局异常，写日志
        var action = context.HttpContext.Request.Path + "";
        if (context.ActionDescriptor is ControllerActionDescriptor act) action = $"{act.ControllerName}/{act.ActionName}";

        _appService.WriteHistory(_App, action, false, ex?.GetTrue() + Environment.NewLine + _args?.ToJson(true), UserHost);
    }
    #endregion

    #region 登录
    /// <summary>应用登录</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost(nameof(Login))]
    public LoginResponse Login(LoginModel model)
    {
        if (model.Code.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.Code));

        // 与心跳、注销使用同一在线会话标识。优先使用客户端标识，避免同IP多实例互相覆盖
        var sessionId = BuildSessionId(model.Code, model.ClientId, UserHost);
        var (app, online, rs) = _appService.Login(model, sessionId, UserHost);

        // HTTP 通道使用 JWT 令牌，与 RPC 通道的登录语义保持一致
        var token = _appService.IssueToken(app.Name, _setting);
        rs.Token = token.AccessToken;
        rs.Expire = _setting.TokenExpire;

        return rs;
    }

    /// <summary>注销</summary>
    /// <param name="reason">原因</param>
    /// <param name="clientId">客户端标识。用于定位同一IP下的不同实例</param>
    /// <returns></returns>
    [HttpGet(nameof(Logout))]
    public ILogoutResponse Logout(String reason, String clientId)
    {
        var online = _appService.GetOnline(_App, BuildSessionId(_App.Name, clientId, UserHost), UserHost);

        return _appService.Logout(_App, online, reason, UserHost);
    }

    /// <summary>心跳。维持在线状态，临近过期时续发令牌</summary>
    /// <param name="request">心跳请求</param>
    /// <param name="clientId">客户端标识。用于定位同一IP下的不同实例</param>
    /// <returns></returns>
    [HttpPost(nameof(Ping))]
    public IPingResponse Ping(PingRequest request, [FromQuery] String clientId)
    {
        var online = _appService.GetOnline(_App, BuildSessionId(_App.Name, clientId, UserHost), UserHost);
        var rs = _appService.Ping(_App, online, request, UserHost);

        // 令牌即将过期时重新颁发，客户端会在下一次心跳后更新
        var token = ApiFilterAttribute.GetToken(HttpContext);
        var tm = _appService.ValidAndIssueToken(_App.Name, token, _setting);
        if (tm != null && rs is PingResponse pr) pr.Token = tm.AccessToken;

        return rs;
    }

    [AllowAnonymous]
    [HttpPost(nameof(Token))]
    public TokenModel Token([FromBody] TokenInModel model)
    {
        var set = _setting;

        if (model.grant_type.IsNullOrEmpty()) model.grant_type = "password";

        var ip = HttpContext.GetUserHost();
        var clientId = model.ClientId;

        try
        {
            // 密码模式
            if (model.grant_type == "password")
            {
                var sessionId = BuildSessionId(model.UserName, clientId, ip);
                var (app, online, rs) = _appService.Login(new LoginModel { Code = model.UserName, Secret = model.Password, ClientId = clientId }, sessionId, ip);

                var tokenModel = _appService.IssueToken(app.Name, set);

                _appService.WriteHistory(app, "Authorize", true, model.ToJson(), ip);

                return tokenModel;
            }
            // 刷新令牌
            else if (model.grant_type == "refresh_token")
            {
                var (app, ex) = _appService.DecodeToken(model.refresh_token, set.TokenSecret);

                if (ex != null)
                {
                    _appService.WriteHistory(app, "RefreshToken", false, ex.ToString(), ip);
                    throw ex;
                }

                var tokenModel = _appService.IssueToken(app.Name, set);

                //app.WriteHistory("RefreshToken", true, model.refresh_token, olt?.Version, ip, clientId);

                return tokenModel;
            }
            else
            {
                throw new NotSupportedException($"未支持 grant_type={model.grant_type}");
            }
        }
        catch (Exception ex)
        {
            var app = App.FindByName(model.UserName);
            _appService.WriteHistory(app, "Authorize", false, ex.ToString(), ip);

            throw;
        }
    }

    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    [HttpGet(nameof(GetPeers))]
    public PeerModel[] GetPeers() => _appService.GetPeers(_App);

    /// <summary>下行通知。建立WebSocket长连接，接收作业变更与消息到达信号</summary>
    /// <remarks>信号只用于降低延迟，客户端收到后仍需通过 HTTP 拉取权威数据</remarks>
    /// <returns></returns>
    [HttpGet(nameof(Notify))]
    public async Task Notify()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        // 同一客户端实例重连时使用相同会话编码，SessionManager 会替换旧连接
        var clientId = HttpContext.Request.Query["clientId"] + "";
        using var session = new WsCommandSession(socket)
        {
            Code = clientId.IsNullOrEmpty() ? $"{_App.Name}@{Rand.NextString(8)}" : BuildSessionId(_App.Name, clientId, UserHost),
            ServiceProvider = HttpContext.RequestServices,
            Tracer = HttpContext.RequestServices.GetService<ITracer>(),
        };
        _notifyManager.Add(session);

        var context = new DeviceContext
        {
            Code = _App.Name,
            ClientId = session.Code,
            UserHost = UserHost,
        };
        context["HttpContext"] = HttpContext;
        context["Span"] = DefaultSpan.Current;

        await session.WaitAsync(context, HttpContext.RequestAborted);
    }
    #endregion

    #region 业务
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    [HttpGet(nameof(GetJobs))]
    public IJob[] GetJobs() => _jobService.GetJobs(_App);

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    [HttpPost(nameof(AddJobs))]
    public String[] AddJobs(JobModel[] jobs)
    {
        if (jobs == null || jobs.Length == 0) return [];

        return _jobService.AddJobs(_App, jobs);
    }

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="json">作业参数。仅提交需要修改的字段</param>
    /// <returns></returns>
    [HttpPost(nameof(SetJob))]
    public IJob SetJob([FromBody] JsonElement json)
    {
        var model = json.GetRawText().ToJsonEntity<JobModel>();

        // 仅把提交过来的字段作为可修改参数，与 RPC 通道保持一致
        var parameters = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in json.EnumerateObject())
        {
            parameters[item.Name] = null;
        }

        return _jobService.SetJob(_App, model, parameters);
    }

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [HttpPost(nameof(Acquire))]
    public ITask[] Acquire(AcquireModel model)
    {
        var job = model.Job?.Trim();
        if (job.IsNullOrEmpty()) return [];

        //var sessionId = $"{_App.Name}@{UserHost}";
        var online = _appService.GetOnline(_App, null, UserHost);

        return _jobService.Acquire(_App, model, online);
    }

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [HttpPost(nameof(Produce))]
    public Int32 Produce(ProduceModel model)
    {
        var messages = model?.Messages?.Where(e => !e.IsNullOrEmpty()).Distinct().ToArray();
        if (messages == null || messages.Length == 0) return 0;

        return _jobService.Produce(_App, model);
    }

    /// <summary>报告状态（进度、成功、错误）</summary>
    /// <param name="task"></param>
    /// <returns></returns>
    [HttpPost(nameof(Report))]
    public Boolean Report(TaskResult task)
    {
        if (task == null || task.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + task?.ID);

        //var sessionId = $"{_App.Name}@{UserHost}";
        var online = _appService.GetOnline(_App, null, UserHost);
        return _jobService.Report(_App, task, online);
    }
    #endregion
}
