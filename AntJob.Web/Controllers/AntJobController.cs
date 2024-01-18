using System.Reflection;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server;
using AntJob.Server.Services;
using AntJob.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife;
using NewLife.Caching;
using NewLife.Cube;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Serialization;
using IActionFilter = Microsoft.AspNetCore.Mvc.Filters.IActionFilter;

namespace AntJob.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class AntJobController : ControllerBase, IActionFilter
{
    /// <summary>令牌</summary>
    public String Token { get; private set; }

    /// <summary>用户主机</summary>
    public String UserHost => HttpContext.GetUserHost();

    private App _App;
    private IDictionary<String, Object> _args;
    private AntJobSetting _setting;
    private readonly AppService _appService;
    private readonly JobService _jobService;
    private readonly ICacheProvider _cacheProvider;

    #region 构造
    public AntJobController(AppService appService, JobService jobService, AntJobSetting setting)
    {
        _appService = appService;
        _jobService = jobService;
        _setting = setting;
    }

    void IActionFilter.OnActionExecuting(ActionExecutingContext context)
    {
        _args = context.ActionArguments;

        var token = Token = ApiFilterAttribute.GetToken(context.HttpContext);

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
        if (model.User.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.User));

        var (app, rs) = _appService.Login(model, UserHost);

        return rs;
    }

    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    [HttpGet(nameof(GetPeers))]
    public PeerModel[] GetPeers() => _appService.GetPeers(_App);
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
        if (jobs == null || jobs.Length == 0) return new String[0];

        return _jobService.AddJobs(_App, jobs);
    }

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [HttpPost(nameof(Acquire))]
    public ITask[] Acquire(AcquireModel model)
    {
        var job = model.Job?.Trim();
        if (job.IsNullOrEmpty()) return new TaskModel[0];

        return _jobService.Acquire(_App, model, UserHost);
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

        return _jobService.Report(_App, task, UserHost);
    }
    #endregion
}
