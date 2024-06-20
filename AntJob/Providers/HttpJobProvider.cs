using AntJob.Data;
using AntJob.Handlers;
using AntJob.Models;
using NewLife;
using NewLife.Http;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Threading;

namespace AntJob.Providers;

/// <summary>Http任务提供者</summary>
public class HttpJobProvider : JobProvider
{
    #region 属性
    /// <summary>调试，打开编码日志</summary>
    public Boolean Debug { get; set; }

    /// <summary>调度中心地址</summary>
    public String Server { get; set; }

    /// <summary>应用编号</summary>
    public String AppId { get; set; }

    /// <summary>应用密钥</summary>
    public String Secret { get; set; }

    /// <summary>客户端</summary>
    public ApiHttpClient Client { get; set; }

    /// <summary>邻居伙伴。用于应用判断自身有多少个实例在运行</summary>
    public IPeer[] Peers { get; private set; }

    /// <summary>性能跟踪器</summary>
    public ITracer Tracer { get; set; }
    #endregion

    #region 构造
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _timer.TryDispose();
        _timer = null;
    }
    #endregion

    #region 启动停止
    /// <summary>初始化</summary>
    public void Init()
    {
        var svr = Server?.Split(",").Where(e => e.StartsWithIgnoreCase("http://", "https://")).Join(",");

        // 使用配置中心账号
        var client = new ApiHttpClient(svr)
        {
            Tracer = Tracer,
        };

        client.Filter = new TokenHttpFilter
        {
            Action = "/AntJob/Login",
            UserName = AppId,
            Password = Secret,
        };

        // 断开前一个连接
        Client.TryDispose();
        Client = client;
    }

    /// <summary>开始</summary>
    public override void Start()
    {
        if (Client == null) Init();

        var bs = Schedule?.Handlers;

        // 遍历所有处理器，添加作业到调度中心
        //var jobs = GetJobs(ws.Select(e => e.Name).ToArray());
        var list = new List<IJob>();
        foreach (var handler in bs)
        {
            var job = handler.Job ?? new JobModel();

            job.Name = handler.Name;
            job.ClassName = handler.GetType().FullName;
            job.Mode = handler.Mode;

            // 描述
            if (job is JobModel job2)
            {
                var dis = handler.GetType().GetDisplayName();
                if (!dis.IsNullOrEmpty()) job2.DisplayName = dis;
                var des = handler.GetType().GetDescription();
                if (!des.IsNullOrEmpty()) job2.Description = des;

                if (handler is MessageHandler mhandler) job2.Topic = mhandler.Topic;
            }

            list.Add(job);
        }
        if (list.Count > 0) Client.Post<Object>("/AntJob/AddJobs", new { jobs = list.ToArray() });

        // 定时更新邻居
        _timer = new TimerX(DoCheckPeer, null, 1_000, 30_000) { Async = true };
    }

    /// <summary>停止</summary>
    public override void Stop()
    {
        // 断开前一个连接
        Client.TryDispose();
        Client = null;
    }
    #endregion

    #region 作业消息控制
    private IJob[] _jobs;
    private DateTime _NextGetJobs;
    /// <summary>获取所有作业名称</summary>
    /// <returns></returns>
    public override IJob[] GetJobs()
    {
        // 周期性获取，避免请求过快
        var now = TimerX.Now;
        if (_jobs == null || _NextGetJobs <= now)
        {
            _NextGetJobs = now.AddSeconds(5);

            _jobs = Client.Get<JobModel[]>("/AntJob/GetJobs");
        }

        return _jobs;
    }

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public override ITask[] Acquire(IJob job, String topic, Int32 count) => Client.Post<TaskModel[]>("/AntJob/Acquire", new AcquireModel { Job = job.Name, Topic = topic, Count = count });

    /// <summary>生产消息</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="messages">消息集合</param>
    /// <param name="option">消息选项</param>
    /// <returns></returns>
    public override Int32 Produce(String job, String topic, String[] messages, MessageOption option = null)
    {
        if (topic.IsNullOrEmpty() || messages == null || messages.Length < 1) return 0;

        var model = new ProduceModel
        {
            Job = job,
            Topic = topic,
            Messages = messages,
        };
        if (option != null)
        {
            model.DelayTime = option.DelayTime;
            model.Unique = option.Unique;
        }

        return Client.Post<Int32>("/AntJob/Product", model);
    }
    #endregion

    #region 报告状态
    /// <summary>报告进度，每个任务多次调用</summary>
    /// <param name="ctx">上下文</param>
    public override void Report(JobContext ctx)
    {
        // 不用上报抽取中
        if (ctx.Status == JobStatus.抽取中) return;

        if (ctx?.Result is not TaskResult task) return;

        // 区分抽取和处理
        task.Status = ctx.Status;

        task.Speed = ctx.Speed;
        task.Total = ctx.Total;
        task.Success = ctx.Success;

        if (ctx.NextTime.Year > 2000) task.NextTime = ctx.NextTime.ToUniversalTime();

        Report(ctx.Handler.Job, task);
    }

    /// <summary>完成任务，每个任务只调用一次</summary>
    /// <param name="ctx">上下文</param>
    public override void Finish(JobContext ctx)
    {
        if (ctx?.Result is not TaskResult task) return;

        task.Speed = ctx.Speed;
        task.Total = ctx.Total;
        task.Success = ctx.Success;
        task.Times++;

        // 区分正常完成还是错误终止
        if (ctx.Error != null)
        {
            task.Error++;
            task.Status = JobStatus.错误;

            var ex = ctx.Error?.GetTrue();
            if (ex != null)
            {
                var msg = ctx.Error.GetMessage();
                if (msg.Contains("Exception:")) msg = msg.Substring("Exception:").Trim();
                task.Message = msg;
            }
        }
        else if (task.Status <= JobStatus.处理中)
        {
            task.Status = JobStatus.完成;
        }
        task.Cost = (Int32)Math.Round(ctx.Cost / 1000);
        if (task.Message.IsNullOrEmpty()) task.Message = ctx.Remark;

        task.Key = ctx.Key;

        Report(ctx.Handler.Job, task);
    }

    private void Report(IJob job, ITaskResult task)
    {
        try
        {
            Client.Post<Boolean>("/AntJob/Report", task);
        }
        catch (Exception ex)
        {
            WriteLog("[{0}]的[{1}]状态报告失败！{2}", job, task.Status, ex.GetTrue().Message);
        }
    }
    #endregion

    #region 邻居
    private TimerX _timer;
    private void DoCheckPeer(Object state)
    {
        var ps = Client?.Get<PeerModel[]>("/AntJob/GetPeers");
        if (ps == null || ps.Length == 0) return;

        var old = (Peers ?? []).ToList();
        foreach (var item in ps)
        {
            var pr = old.FirstOrDefault(e => e.Instance == item.Instance);
            if (pr == null)
                WriteLog("[{0}]上线！{1}", item.Instance, item.Machine);
            else
                old.Remove(pr);
        }
        foreach (var item in old)
        {
            WriteLog("[{0}]下线！{1}", item.Instance, item.Machine);
        }

        Peers = ps;
    }
    #endregion
}