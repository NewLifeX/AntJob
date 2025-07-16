using System.Net;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server.Services;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Net;
using NewLife.Remoting;
using NewLife.Remoting.Models;
using NewLife.Threading;

namespace AntJob.Server;

/// <summary>蚂蚁服务层，Rpc接口服务</summary>
/// <remarks>
/// 该服务层主要用于蚂蚁调度器与蚂蚁处理器之间的通讯，以及蚂蚁处理器与蚂蚁数据中心之间的通讯。
/// 服务注册到内部对象容器IObjectContainer，要求宿主ApiServer指定ServiceProvider为IObjectContainer。
/// </remarks>
[Api(null)]
class AntService(AppService appService, JobService jobService) : IApi, IActionFilter
{
    #region 属性
    /// <summary>本地节点</summary>
    public static EndPoint Local { get; set; }

    public IApiSession Session { get; set; }

    private App _App;
    private AppOnline _Online;
    private INetSession _Net;
    #endregion

    #region 构造
    void IActionFilter.OnActionExecuting(ControllerContext filterContext)
    {
        _Net = Session as INetSession;

        var act = filterContext.ActionName;
        if (act == nameof(Login)) return;

        if (Session["App"] is not App app)
            throw new ApiException(ApiCode.Unauthorized, $"{_Net.Remote}未登录！不能执行{act}");

        _App = app;

        if (Session["AppOnline"] is not AppOnline online)
        {
            var remote = _Net.Remote;
            online = appService.GetOnline(app, remote + "", remote.Host);
        }

        _Online = online;

        online.UpdateTime = TimerX.Now;
        online.SaveAsync();
    }

    void IActionFilter.OnActionExecuted(ControllerContext filterContext)
    {
        if (filterContext.Exception != null && !filterContext.ExceptionHandled)
        {
            // 显示错误
            var ex = filterContext.Exception;
            if (ex != null)
            {
                if (ex is ApiException)
                    XTrace.Log.Error(ex.Message);
                else
                    XTrace.WriteException(ex);

                _Net = Session as INetSession;
                var ip = _Net.Remote.Host;
                appService.WriteHistory(_App, filterContext.ActionName, false, ex.GetMessage(), ip);
            }
        }
    }
    #endregion

    #region 登录心跳
    /// <summary>应用登录</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Login))]
    public LoginResponse Login(LoginModel model)
    {
        // 兼容旧版本
        if (model.Code.IsNullOrEmpty() && !model.User.IsNullOrEmpty()) model.Code = model.User;
        if (model.Secret.IsNullOrEmpty() && !model.Pass.IsNullOrEmpty()) model.Secret = model.Pass;

        if (model.Code.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.Code));

        var remote = _Net.Remote;
        var (app, online, rs) = appService.Login(model, null, remote.Host);

        // 记录当前用户
        Session["App"] = app;
        Session["AppOnline"] = online;

        return rs;
    }

    [Api(nameof(Logout))]
    public ILogoutResponse Logout(String reason) => appService.Logout(_App, _Online, reason, _Net.Remote.Host);

    [Api(nameof(Ping))]
    public IPingResponse Ping(PingRequest request) => appService.Ping(_App, _Online, request, _Net.Remote.Host);

    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    [Api(nameof(GetPeers))]
    public PeerModel[] GetPeers() => appService.GetPeers(_App);
    #endregion

    #region 业务
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    [Api(nameof(GetJobs))]
    public IJob[] GetJobs() => jobService.GetJobs(_App);

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    [Api(nameof(AddJobs))]
    public String[] AddJobs(JobModel[] jobs)
    {
        if (jobs == null || jobs.Length == 0) return [];

        return jobService.AddJobs(_App, jobs);
    }

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <returns></returns>
    [Api(nameof(SetJob))]
    public IJob SetJob(JobModel job) => jobService.SetJob(_App, job, ControllerContext.Current.Parameters);

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Acquire))]
    public ITask[] Acquire(AcquireModel model)
    {
        var span = DefaultSpan.Current;
        if (span != null) span.Value = 0;

        var job = model.Job?.Trim();
        if (job.IsNullOrEmpty()) return [];

        var tasks = jobService.Acquire(_App, model, _Online);

        // 记录申请到的任务数
        if (span != null) span.Value = tasks?.Length ?? 0;

        return tasks;
    }

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Produce))]
    public Int32 Produce(ProduceModel model)
    {
        var messages = model?.Messages?.Where(e => !e.IsNullOrEmpty()).Distinct().ToArray();
        if (messages == null || messages.Length == 0) return 0;

        return jobService.Produce(_App, model);
    }

    /// <summary>报告状态（进度、成功、错误）</summary>
    /// <param name="task"></param>
    /// <returns></returns>
    [Api(nameof(Report))]
    public Boolean Report(TaskResult task)
    {
        if (task == null || task.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + task?.ID);

        return jobService.Report(_App, task, _Online);
    }
    #endregion
}