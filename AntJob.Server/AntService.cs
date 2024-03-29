﻿using System.Net;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using AntJob.Server.Services;
using NewLife;
using NewLife.Caching;
using NewLife.Data;
using NewLife.Log;
using NewLife.Net;
using NewLife.Remoting;
using NewLife.Threading;
using XCode;

namespace AntJob.Server;

/// <summary>蚂蚁服务层，Rpc接口服务</summary>
/// <remarks>
/// 该服务层主要用于蚂蚁调度器与蚂蚁工作器之间的通讯，以及蚂蚁工作器与蚂蚁数据中心之间的通讯。
/// 服务注册到内部对象容器IObjectContainer，要求宿主ApiServer指定ServiceProvider为IObjectContainer。
/// </remarks>
[Api(null)]
class AntService : IApi, IActionFilter
{
    #region 属性
    /// <summary>本地节点</summary>
    public static EndPoint Local { get; set; }

    public IApiSession Session { get; set; }

    private App _App;
    private INetSession _Net;
    private readonly AppService _appService;
    private readonly JobService _jobService;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;
    private readonly ILog _log;
    #endregion

    #region 构造
    public AntService(AppService appService, JobService jobService, ICacheProvider cacheProvider, ITracer tracer, ILog log)
    {
        _appService = appService;
        _jobService = jobService;
        _cacheProvider = cacheProvider;
        _tracer = tracer;
        _log = log;
    }

    void IActionFilter.OnActionExecuting(ControllerContext filterContext)
    {
        _Net = Session as INetSession;

        var act = filterContext.ActionName;
        if (act == nameof(Login)) return;

        if (Session["App"] is App app)
        {
            _App = app;

            var ip = _Net.Remote.Host;
            var online = _appService.GetOnline(app, ip);
            online.UpdateTime = TimerX.Now;
            online.SaveAsync();
        }
        else
        {
            throw new ApiException(401, $"{_Net.Remote}未登录！不能执行{act}");
        }
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
                _appService.WriteHistory(_App, filterContext.ActionName, false, ex.GetMessage(), ip);
            }
        }
    }
    #endregion

    #region 登录
    /// <summary>应用登录</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Login))]
    public LoginResponse Login(LoginModel model)
    {
        if (model.User.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.User));

        var (app, rs) = _appService.Login(model, _Net.Remote.Host);

        // 记录当前用户
        Session["App"] = app;

        return rs;
    }

    /// <summary>获取当前应用的所有在线实例</summary>
    /// <returns></returns>
    [Api(nameof(GetPeers))]
    public PeerModel[] GetPeers() => _appService.GetPeers(_App);
    #endregion

    #region 业务
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    [Api(nameof(GetJobs))]
    public IJob[] GetJobs() => _jobService.GetJobs(_App);

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    [Api(nameof(AddJobs))]
    public String[] AddJobs(JobModel[] jobs)
    {
        if (jobs == null || jobs.Length == 0) return new String[0];

        return _jobService.AddJobs(_App, jobs);
    }

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Acquire))]
    public ITask[] Acquire(AcquireModel model)
    {
        var job = model.Job?.Trim();
        if (job.IsNullOrEmpty()) return new TaskModel[0];

        var ip = _Net.Remote.Host;
        return _jobService.Acquire(_App, model, ip);
    }

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    [Api(nameof(Produce))]
    public Int32 Produce(ProduceModel model)
    {
        var messages = model?.Messages?.Where(e => !e.IsNullOrEmpty()).Distinct().ToArray();
        if (messages == null || messages.Length == 0) return 0;

        return _jobService.Produce(_App, model);
    }

    /// <summary>报告状态（进度、成功、错误）</summary>
    /// <param name="task"></param>
    /// <returns></returns>
    [Api(nameof(Report))]
    public Boolean Report(TaskResult task)
    {
        if (task == null || task.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + task?.ID);

        var ip = _Net.Remote.Host;
        return _jobService.Report(_App, task, ip);
    }
    #endregion
}