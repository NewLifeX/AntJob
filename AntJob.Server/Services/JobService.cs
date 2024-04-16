using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using NewLife;
using NewLife.Caching;
using NewLife.Data;
using NewLife.Log;
using NewLife.Serialization;
using NewLife.Threading;
using XCode;

namespace AntJob.Server.Services;

public class JobService
{
    private readonly AppService _appService;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;
    private readonly ILog _log;

    public JobService(AppService appService, ICacheProvider cacheProvider, ITracer tracer, ILog log)
    {
        _appService = appService;
        _cacheProvider = cacheProvider;
        _tracer = tracer;
        _log = log;
    }

    #region 业务
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    public IJob[] GetJobs(App app)
    {
        var jobs = Job.FindAllByAppID(app.ID);

        return jobs.Select(e => e.ToModel()).ToArray();
    }

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    public String[] AddJobs(App app, JobModel[] jobs)
    {
        if (jobs == null || jobs.Length == 0) return new String[0];

        var myJobs = Job.FindAllByAppID(app.ID);
        var list = new List<String>();
        foreach (var item in jobs)
        {
            var jb = myJobs.FirstOrDefault(e => e.Name.EqualIgnoreCase(item.Name));
            jb ??= new Job
            {
                AppID = app.ID,
                Name = item.Name,
                Enable = item.Enable,
                Start = item.Start,
                End = item.End,
                Offset = item.Offset,
                Step = item.Step,
                BatchSize = item.BatchSize,
                MaxTask = item.MaxTask,
                Mode = item.Mode,
                MaxError = 100,
            };

            if (item.Mode > 0) jb.Mode = item.Mode;
            if (!item.DisplayName.IsNullOrEmpty()) jb.DisplayName = item.DisplayName;
            if (!item.Description.IsNullOrEmpty()) jb.Remark = item.Description;
            if (!item.ClassName.IsNullOrEmpty()) jb.ClassName = item.ClassName;
            if (jb.Topic.IsNullOrEmpty()) jb.Topic = item.Topic;

            if (jb.Save() != 0)
            {
                // 更新作业数
                jb.SaveAsync();

                list.Add(jb.Name);
            }
        }

        return list.ToArray();
    }

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public ITask[] Acquire(App app, AcquireModel model, String ip)
    {
        var job = model.Job?.Trim();
        if (job.IsNullOrEmpty()) return new TaskModel[0];

        if (app == null) return new TaskModel[0];

        // 应用停止发放作业
        app = App.FindByID(app.ID) ?? app;
        if (!app.Enable) return new TaskModel[0];
        var jb = app.Jobs.FirstOrDefault(e => e.Name == job);

        // 全局锁，确保单个作业只有一个线程在分配作业
        using var ck = _cacheProvider.AcquireLock($"antjob:lock:{jb.ID}", 15_000);

        // 找到作业。为了确保能够快速拿到新的作业参数，这里做二次查询
        if (jb != null)
            jb = Job.Find(Job._.ID == jb.ID);
        else
            jb = Job.FindByAppIDAndName(app.ID, job);

        if (jb == null) throw new XException($"应用[{app.ID}/{app.Name}]下未找到作业[{job}]");
        if (jb.Step == 0 || jb.Start.Year <= 2000) throw new XException("作业[{0}/{1}]未设置开始时间或步进", jb.ID, jb.Name);

        var online = _appService.GetOnline(app, ip);

        var list = new List<JobTask>();

        // 每分钟检查一下错误任务和中断任务
        CheckErrorTask(app, jb, model.Count, list, ip);

        // 错误项不够时，增加切片
        if (list.Count < model.Count)
        {
            //var ps = ControllerContext.Current.Parameters;
            var server = online.Name;
            var pid = online.ProcessId;
            //var topic = ps["topic"] + "";

            switch (jb.Mode)
            {
                case JobModes.Message:
                    list.AddRange(jb.AcquireMessage(model.Topic, server, ip, pid, model.Count - list.Count, _cacheProvider.Cache));
                    break;
                case JobModes.Data:
                case JobModes.Time:
                //case JobModes.CSharp:
                //case JobModes.Sql:
                default:
                    {
                        // 如果能够切片，则查询数据库后进入，避免缓存导致重复
                        if (jb.TrySplit(jb.Start, jb.Step, out var end))
                        {
                            // 申请任务前，不能再查数据库，那样子会导致多线程脏读，从而出现多客户端分到相同任务的情况
                            //jb = Job.FindByKey(jb.ID);
                            list.AddRange(jb.Acquire(server, ip, pid, model.Count - list.Count, _cacheProvider.Cache));
                        }
                    }
                    break;
            }
        }

        // 记录状态
        online.Tasks += list.Count;
        online.SaveAsync();

        return list.Select(e => e.ToModel()).ToArray();
    }

    private void CheckErrorTask(App app, Job jb, Int32 count, List<JobTask> list, String ip)
    {
        // 每分钟检查一下错误任务和中断任务
        var nextKey = $"antjob:NextAcquireOld_{jb.ID}";
        var now = TimerX.Now;
        var next = _cacheProvider.Cache.Get<DateTime>(nextKey);
        if (next < now)
        {
            var online = _appService.GetOnline(app, ip);

            next = now.AddSeconds(60);
            list.AddRange(jb.AcquireOld(online.Server, ip, online.ProcessId, count, _cacheProvider.Cache));

            if (list.Count > 0)
            {
                // 既然有数据，待会还来
                next = now;

                var n1 = list.Count(e => e.Status == JobStatus.错误 || e.Status == JobStatus.取消);
                var n2 = list.Count(e => e.Status == JobStatus.就绪 || e.Status == JobStatus.抽取中 || e.Status == JobStatus.处理中);
                _log.Info("作业[{0}/{1}]准备处理[{2}]个错误和[{3}]超时任务 [{4}]", app, jb.Name, n1, n2, list.Join(",", e => e.ID + ""));
            }
            else
                _cacheProvider.Cache.Set(nextKey, next);
        }
    }

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public Int32 Produce(App app, ProduceModel model)
    {
        var messages = model?.Messages?.Where(e => !e.IsNullOrEmpty()).Distinct().ToArray();
        if (messages == null || messages.Length == 0) return 0;

        // 去重过滤
        if (model.Unique)
        {
            // 如果消息较短，采用缓存做去重过滤
            if (messages.All(e => e.Length < 64))
            {
                var msgs = new List<String>();
                foreach (var item in messages)
                {
                    var key = $"antjob:{app.ID}:{model.Topic}:{item}";
                    if (_cacheProvider.Cache.Add(key, item, 2 * 3600)) msgs.Add(item);
                }
                messages = msgs.ToArray();
            }
            else
            {
                messages = AppMessage.Filter(app.ID, model.Topic, messages);
                if (messages.Length == 0) return 0;
            }
        }

        var ms = new List<AppMessage>();

        var total = 0;
        var now = DateTime.Now;
        // 延迟需要基于任务开始时间，而不能用使用当前时间，防止回头跑数据时无法快速执行
        var dTime = now.AddSeconds(model.DelayTime);

        var jb = Job.FindByAppIDAndName(app.ID, model.Job);
        var snow = AppMessage.Meta.Factory.Snow;
        foreach (var item in messages)
        {
            var jm = new AppMessage
            {
                Id = snow.NewId(),
                AppID = app.ID,
                JobID = jb == null ? 0 : jb.ID,
                Topic = model.Topic,
                Data = item,
            };

            jm.CreateTime = jm.UpdateTime = now;

            // 雪花Id直接指定消息在未来的消费时间
            if (model.DelayTime > 0)
            {
                jm.Id = snow.NewId(dTime);
                jm.UpdateTime = dTime;
            }

            ms.Add(jm);
        }

        // 记录消息积压数
        total = ms.Insert();

        // 增加消息数
        if (total < 0) total = messages.Length;
        if (total > 0)
        {
            var job2 = app.Jobs?.FirstOrDefault(e => e.Topic == model.Topic);
            if (job2 != null)
            {
                job2.MessageCount += total;
                job2.SaveAsync();
            }

            app.MessageCount += total;
            app.SaveAsync();
        }

        return total;
    }
    #endregion

    #region 状态报告
    /// <summary>报告状态（进度、成功、错误）</summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public Boolean Report(App app, TaskResult task, String ip)
    {
        if (task == null || task.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + task?.ID);

        // 判断是否有权

        var jt = JobTask.FindByID(task.ID) ?? throw new InvalidOperationException($"找不到任务[{task.ID}]");
        var job = Job.FindByID(jt.JobID);
        if (job == null || job.AppID != app.ID)
        {
            _log.Info(task.ToJson());
            throw new InvalidOperationException($"应用[{app}]无权操作作业[{job}#{jt}]");
        }

        // 只有部分字段允许客户端修改
        if (task.Status > 0) jt.Status = task.Status;

        jt.Speed = task.Speed;
        jt.Total = task.Total;
        jt.Success = task.Success;
        jt.Cost = task.Cost;
        jt.Key = task.Key;
        jt.Message = task.Message;

        // 已终结的作业，汇总统计
        if (task.Status == JobStatus.完成 || task.Status == JobStatus.错误)
        {
            jt.Times++;

            SetJobFinish(job, jt);

            // 记录状态
            _appService.UpdateOnline(app, jt, ip);
        }
        if (task.Status == JobStatus.错误)
        {
            SetJobError(job, jt);

            jt.Error++;
            //ji.Message = err.Message;

            // 出错时判断如果超过最大错误数，则停止作业
            CheckMaxError(app, job);
        }

        // 从创建到完成的全部耗时
        var ts = DateTime.Now - jt.CreateTime;
        jt.FullCost = (Int32)ts.TotalSeconds;

        jt.SaveAsync();
        //ji.Save();

        return true;
    }

    private void SetJobFinish(Job job, JobTask task)
    {
        job.Total += task.Total;
        job.Success += task.Success;
        job.Error += task.Error;
        job.Times++;

        var ths = job.MaxTask;

        var p1 = task.Speed * ths;

        if (p1 > 0)
        {
            // 平均速度
            if (job.Speed > 0)
                job.Speed = (Int32)((job.Speed * 3L + p1) / 4);
            else
                job.Speed = p1;
        }

        job.SaveAsync();
        //job.Save();
    }

    private JobError SetJobError(Job job, JobTask task)
    {
        var err = new JobError
        {
            AppID = job.AppID,
            JobID = job.ID,
            TaskID = task.ID,
            Start = task.Start,
            End = task.End,
            Data = task.Data,

            Server = task.Server,
            ProcessID = task.ProcessID,
            Client = task.Client,

            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
        };

        var msg = task.Message;
        if (!msg.IsNullOrEmpty() && msg.Contains("Exception:")) msg = msg.Substring("Exception:").Trim();
        err.Message = msg;

        err.Insert();

        return err;
    }

    private void CheckMaxError(App app, Job job)
    {
        // 出错时判断如果超过最大错误数，则停止作业
        var maxError = job.MaxError < 1 ? 100 : job.MaxError;
        if (job.Enable && job.Error > maxError)
        {
            job.MaxError = maxError;
            job.Enable = false;

            //job.SaveAsync();
            (job as IEntity).Update();
        }
    }
    #endregion
}
