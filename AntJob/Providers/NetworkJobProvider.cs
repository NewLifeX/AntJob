using AntJob.Data;
using AntJob.Handlers;
using AntJob.Models;
using NewLife;
using NewLife.Remoting.Models;
using NewLife.Serialization;
using NewLife.Threading;

namespace AntJob.Providers;

/// <summary>网络任务提供者</summary>
public class NetworkJobProvider(AntSetting setting) : JobProvider
{
    #region 属性
    /// <summary>配置</summary>
    protected AntSetting Setting { get; } = setting;

    /// <summary>客户端</summary>
    public AntClient Ant { get; set; }

    /// <summary>邻居伙伴。用于应用判断自身有多少个实例在运行</summary>
    public IPeer[] Peers { get; private set; }

    /// <summary>消息型作业空闲时的申请周期（秒）。HTTP 通道由 WebSocket 通知触发，RPC 通道保持轮询</summary>
    public virtual Int32 MessageIdlePeriod => 0;
    #endregion

    #region 构造
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Stop().Wait(5_000);

        _timer.TryDispose();
        _timer = null;
    }
    #endregion

    #region 启动停止
    /// <summary>创建客户端。派生类可注入自定义客户端</summary>
    /// <param name="setting">配置</param>
    /// <returns></returns>
    protected virtual AntClient CreateClient(AntSetting setting) => new(setting);

    /// <summary>下行通知通道连接成功。断线期间可能丢失通知，重连后立即拉取一次</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnNotifyConnected(Object sender, EventArgs e)
    {
        _jobs = null;
        // 重置有效期，避免通知到达时恰好有一次请求在途，旧结果写回缓存后长期不刷新
        _NextGetJobs = DateTime.MinValue;

        var handlers = Schedule?.GetHandlers();
        if (handlers != null)
        {
            foreach (var handler in handlers)
            {
                handler.NextAcquire = DateTime.MinValue;
            }
        }

        Schedule?.WakeUp();
    }

    /// <summary>收到服务端下行信号。清除作业缓存并立即唤醒调度</summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnReceived(Object sender, CommandEventArgs e)
    {
        var command = e.Model?.Command;
        if (command.IsNullOrEmpty()) return;

        switch (command)
        {
            case "JobChanged":
            case "Message":
            case "Reload":
                // 解析主题，消息通知只唤醒订阅该主题的处理器
                var topic = "";
                if (command == "Message")
                {
                    try
                    {
                        var dic = JsonParser.Decode(e.Model?.Argument);
                        if (dic != null && dic.TryGetValue("topic", out var value)) topic = value + "";
                    }
                    catch
                    {
                        // 通知参数异常时按无主题处理，不影响本轮唤醒
                    }
                }

                var handlers = Schedule?.GetHandlers();
                if (handlers != null)
                {
                    foreach (var handler in handlers)
                    {
                        if (command == "Message" && !topic.IsNullOrEmpty() && handler.Job?.Mode == JobModes.Message && !handler.Job.Topic.EqualIgnoreCase(topic)) continue;

                        handler.NextAcquire = DateTime.MinValue;
                    }
                }

                // 让下一次 GetJobs 立即命中服务端，随后立刻执行一轮调度
                _jobs = null;
                _NextGetJobs = DateTime.MinValue;
                Schedule?.WakeUp();
                break;
        }
    }

    /// <summary>开始</summary>
    public override async Task Start()
    {
        WriteLog("正在连接调度中心：{0}", Setting.Server);

        // 使用配置中心账号
        var ant = CreateClient(Setting);
        ant.Tracer = Tracer;
        ant.Log = Log;
        ant.Received += OnReceived;
        ant.NotifyConnectedChanged += OnNotifyConnected;
        await ant.Login();

        // 断开前一个连接
        if (Ant != null)
        {
            await Ant.Logout("new Start");
            Ant.TryDispose();
        }
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
                handler.Tracer ??= Tracer;
                handler.Log ??= Log;
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

            var rs = await Ant.AddJobs(list.ToArray());

            WriteLog("新增作业[{0}]：{1}", rs?.Length, rs.Join());
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
    public override async Task Stop()
    {
        var ant = Ant;
        if (ant != null)
        {
            await ant.Logout(nameof(Stop));

            // 断开前一个连接
            ant.TryDispose();
            Ant = null;
        }
    }
    #endregion

    #region 作业消息控制
    private IJob[] _jobs;
    private IJob[] _baks;
    private DateTime _NextGetJobs;
    /// <summary>获取所有作业名称</summary>
    /// <returns></returns>
    public override async Task<IJob[]> GetJobs()
    {
        // 周期性获取，避免请求过快。HTTP 通道依赖 WebSocket 通知触发即时刷新，轮询只做兜底
        var now = TimerX.Now;
        if (_jobs == null || _NextGetJobs <= now)
        {
            _NextGetJobs = now.AddSeconds(Ant?.IsHttp == true ? 30 : 5);

            var jobs = await Ant.GetJobs();
            if (jobs != null)
            {
                foreach (var job in jobs)
                {
                    // 通信约定UTC，收到后需转为本地时间
                    job.DataTime = job.DataTime.ToLocalTime();
                    if (job.End.Year > 1000)
                        job.End = job.End.ToLocalTime();
                }

                // 备份一份，用于比较
                _baks = jobs.Select(e => ((ICloneable)e).Clone() as IJob).ToArray();
            }

            _jobs = jobs;
        }

        return _jobs;
    }

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public override Task<IJob> SetJob(IJob job)
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

        // 通信约定UTC，时间字段先转UTC再提交；__utc 用于兼容旧版服务端
        if (dic.TryGetValue(nameof(IJob.DataTime), out var dt) && dt is DateTime d1 && d1.Year > 1000)
            dic[nameof(IJob.DataTime)] = d1.ToUniversalTime();
        if (dic.TryGetValue(nameof(IJob.End), out var end) && end is DateTime d2 && d2.Year > 1000)
            dic[nameof(IJob.End)] = d2.ToUniversalTime();
        dic["__utc"] = true;

        return Ant.SetJob(dic);
    }

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public override async Task<ITask[]> Acquire(IJob job, String topic, Int32 count)
    {
        var rs = await Ant.Acquire(job.Name, topic, count);
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
    public override async Task<Int32> Produce(String job, String topic, String[] messages, MessageOption option = null)
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
            model.AppId = option.AppId;
        }

        return await Ant.Produce(model);
    }
    #endregion

    #region 报告状态
    /// <summary>报告进度，每个任务多次调用</summary>
    /// <param name="ctx">上下文</param>
    public override async Task Report(JobContext ctx)
    {
        //// 不用上报抽取中
        //if (ctx.Status == JobStatus.抽取中) return;

        if (ctx?.Result is not TaskResult task) return;

        // 区分抽取和处理
        task.Status = ctx.Status;

        task.Total = ctx.Total;
        task.Success = ctx.Success;
        task.Cost = (Int32)Math.Round(ctx.Cost);
        //task.Speed = ctx.Speed;

        task.Key = ctx.Key;
        task.Remark = ctx.Remark;

        if (ctx.NextTime.Year > 2000) task.NextTime = ctx.NextTime.ToUniversalTime();

        await Report(ctx.Handler.Job, task);
    }

    /// <summary>完成任务，每个任务只调用一次</summary>
    /// <param name="ctx">上下文</param>
    public override async Task Finish(JobContext ctx)
    {
        if (ctx?.Result is not TaskResult task) return;

        task.Total = ctx.Total;
        task.Success = ctx.Success;
        //task.Times++;
        task.Cost = (Int32)Math.Round(ctx.Cost);
        //task.Speed = ctx.Speed;

        if (ctx.NextTime.Year > 2000) task.NextTime = ctx.NextTime.ToUniversalTime();

        // 区分正常完成还是错误终止
        if (ctx.Error != null)
        {
            //task.Error++;
            task.Status = JobStatus.错误;

            var ex = ctx.Error?.GetTrue();
            if (ex != null)
            {
                var msg = ctx.Error.GetMessage();
                var p = msg.IndexOf("Exception:");
                if (p >= 0) msg = msg.Substring(p + "Exception:".Length).Trim();
                task.Remark = msg;
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

        if (task.Remark.IsNullOrEmpty()) task.Remark = ctx.Remark;

        task.Key = ctx.Key;

        await Report(ctx.Handler.Job, task);
    }

    private async Task Report(IJob job, ITaskResult task)
    {
        try
        {
            await Ant.Report(task);
        }
        catch (Exception ex)
        {
            WriteLog("[{0}]的[{1}]状态报告失败！{2}", job, task.Status, ex.GetTrue().Message);
        }
    }
    #endregion

    #region 邻居
    private TimerX _timer;

    private async Task DoCheckPeer(Object state)
    {
        var ps = await Ant?.GetPeers();
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