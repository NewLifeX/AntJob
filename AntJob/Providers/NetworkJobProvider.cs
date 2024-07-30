using AntJob.Data;
using AntJob.Handlers;
using AntJob.Models;
using NewLife;
using NewLife.Threading;

namespace AntJob.Providers;

/// <summary>网络任务提供者</summary>
public class NetworkJobProvider(AntSetting setting) : JobProvider
{
    #region 属性
    /// <summary>客户端</summary>
    public AntClient Ant { get; set; }

    /// <summary>邻居伙伴。用于应用判断自身有多少个实例在运行</summary>
    public IPeer[] Peers { get; private set; }
    #endregion

    #region 构造
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Stop();

        _timer.TryDispose();
        _timer = null;
    }
    #endregion

    #region 启动停止
    /// <summary>开始</summary>
    public override void Start()
    {
        WriteLog("正在连接调度中心：{0}", setting.Server);

        // 使用配置中心账号
        var ant = new AntClient(setting)
        {
            Tracer = Tracer,
            Log = Log,
        };
        ant.Login().Wait();

        // 断开前一个连接
        Ant.TryDispose();
        Ant = ant;

        var bs = Schedule?.Handlers;

        // 遍历所有处理器，添加作业到调度中心
        //var jobs = GetJobs(ws.Select(e => e.Name).ToArray());
        var list = new List<IJob>();
        foreach (var handler in bs)
        {
            // 初始化处理器
            try
            {
                handler.Init();
            }
            catch (Exception ex)
            {
                Log?.Error(ex.Message);
            }

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

            // 改为UTC通信
            if (job.DataTime.Year > 1000)
                job.DataTime = job.DataTime.ToUniversalTime();
            if (job.End.Year > 1000)
                job.End = job.End.ToUniversalTime();

            list.Add(job);
        }
        if (list.Count > 0)
        {
            WriteLog("注册作业[{0}]：{1}", list.Count, list.Join(",", e => e.Name));

            var rs = Ant.AddJobs(list.ToArray());

            WriteLog("注册成功[{0}]：{1}", rs?.Length, rs.Join());
        }

        // 通信完成，改回来本地时间
        foreach (var handler in bs)
        {
            var job = handler.Job;
            if (job != null)
            {
                job.DataTime = job.DataTime.ToLocalTime();
                if (job.End.Year > 1000)
                    job.End = job.End.ToLocalTime();
            }
        }

        // 定时更新邻居
        _timer = new TimerX(DoCheckPeer, null, 1_000, 30_000) { Async = true };
    }

    /// <summary>停止</summary>
    public override void Stop()
    {
        Ant?.Logout(nameof(Stop)).Wait(1_000);

        // 断开前一个连接
        Ant.TryDispose();
        Ant = null;
    }
    #endregion

    #region 作业消息控制
    private IJob[] _jobs;
    private IJob[] _baks;
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

            _jobs = Ant.GetJobs();

            if (_jobs != null)
            {
                foreach (var job in _jobs)
                {
                    // 通信约定UTC，收到后需转为本地时间
                    job.DataTime = job.DataTime.ToLocalTime();
                    if (job.End.Year > 1000)
                        job.End = job.End.ToLocalTime();
                }

                // 备份一份，用于比较
                _baks = _jobs.Select(e => ((ICloneable)e).Clone() as IJob).ToArray();
            }
        }

        return _jobs;
    }

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public override IJob SetJob(IJob job)
    {
        var dic = job.ToDictionary();
        var old = _baks?.FirstOrDefault(e => e.Name == job.Name);
        old ??= new JobModel();

        var dic2 = old.ToDictionary();
        foreach (var item in dic2)
        {
            if (item.Key == nameof(job.Name)) continue;

            // 未修改的不要传递过去
            if (dic.TryGetValue(item.Key, out var value) && Equals(value, item.Value))
                dic.Remove(item.Key);
        }

        return Ant.SetJob(dic);
    }

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public override ITask[] Acquire(IJob job, String topic, Int32 count)
    {
        var rs = Ant.Acquire(job.Name, topic, count);
        if (rs != null)
        {
            foreach (var task in rs)
            {
                // 通信约定UTC，收到后需转为本地时间
                task.DataTime = task.DataTime.ToLocalTime();
                if (task.End.Year > 1000)
                    task.End = task.End.ToLocalTime();
            }
        }

        return rs;
    }

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

        return Ant.Produce(model);
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

        if (ctx.NextTime.Year > 2000) task.NextTime = ctx.NextTime.ToUniversalTime();

        // 区分正常完成还是错误终止
        if (ctx.Error != null)
        {
            task.Error++;
            task.Status = JobStatus.错误;

            var ex = ctx.Error?.GetTrue();
            if (ex != null)
            {
                var msg = ctx.Error.GetMessage();
                var p = msg.IndexOf("Exception:");
                if (p >= 0) msg = msg.Substring(p + "Exception:".Length).Trim();
                task.Message = msg;
            }
        }
        else if (ctx.Status == JobStatus.延迟)
        {
            task.Status = JobStatus.延迟;
        }
        else
        {
            task.Status = JobStatus.完成;
        }

        task.Cost = (Int32)Math.Round(ctx.Cost);
        if (task.Message.IsNullOrEmpty()) task.Message = ctx.Remark;

        task.Key = ctx.Key;

        Report(ctx.Handler.Job, task);
    }

    private void Report(IJob job, ITaskResult task)
    {
        try
        {
            Ant.Report(task);
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
        var ps = Ant?.GetPeers();
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