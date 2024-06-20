﻿using System;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using NewLife;
using NewLife.Caching;
using NewLife.Log;
using NewLife.Serialization;
using NewLife.Threading;
using XCode;
using XCode.DataAccessLayer;

namespace AntJob.Server.Services;

public class JobService(AppService appService, ICacheProvider cacheProvider, ITracer tracer, ILog log)
{
    private readonly AppService _appService = appService;
    private readonly ICacheProvider _cacheProvider = cacheProvider;
    private readonly ITracer _tracer = tracer;
    private readonly ILog _log = log;

    #region 业务
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    public IJob[] GetJobs(App app)
    {
        var jobs = Job.FindAllByAppID(app.ID);

        //return jobs.Select(e => e.ToModel()).ToArray();

        // 服务端下发的时间，约定UTC
        var rs = new List<IJob>();
        foreach (var job in jobs)
        {
            var model = job.ToModel();
            model.DataTime = model.DataTime.ToUniversalTime();
            if (model.End.Year > 1000)
                model.End = model.End.ToUniversalTime();

            rs.Add(model);
        }

        return rs.ToArray();
    }

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    public String[] AddJobs(App app, JobModel[] jobs)
    {
        if (jobs == null || jobs.Length == 0) return [];

        var myJobs = Job.FindAllByAppID(app.ID);
        var list = new List<String>();
        foreach (var model in jobs)
        {
            // 客户端上报的时间，约定UTC，需要转为本地时间
            var job = myJobs.FirstOrDefault(e => e.Name.EqualIgnoreCase(model.Name));
            job ??= new Job
            {
                AppID = app.ID,
                Name = model.Name,
                Enable = model.Enable,
                DataTime = model.DataTime.ToLocalTime(),
                End = model.End.Year > 1000 ? model.End.ToLocalTime() : model.End,
                Offset = model.Offset,
                Step = model.Step,
                BatchSize = model.BatchSize,
                MaxTask = model.MaxTask,
                Mode = model.Mode,
                MaxError = 100,
            };

            if (model.Mode > 0) job.Mode = model.Mode;
            if (!model.DisplayName.IsNullOrEmpty()) job.DisplayName = model.DisplayName;
            if (!model.Description.IsNullOrEmpty()) job.Remark = model.Description;
            if (!model.ClassName.IsNullOrEmpty()) job.ClassName = model.ClassName;
            if (job.Cron.IsNullOrEmpty()) job.Cron = model.Cron;
            if (job.Topic.IsNullOrEmpty()) job.Topic = model.Topic;

            // 添加定时作业时，计算下一次执行时间
            if (job.ID == 0 && job.Mode == JobModes.Time)
            {
                if (job.Step <= 0 && job.Cron.IsNullOrEmpty()) continue;

                // 计算下一次执行时间
                var cron = new Cron(job.Cron);
                var time = job.DataTime.Year > 2000 ? job.DataTime : DateTime.Now;
                job.DataTime = cron.GetNext(time);

                if (job.Step <= 0) job.Step = 30;
            }

            if (job.Save() != 0)
            {
                // 更新作业数
                job.SaveAsync();

                list.Add(job.Name);
            }
        }

        return list.ToArray();
    }

    /// <summary>申请作业任务</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public ITask[] Acquire(App app, AcquireModel model, String ip)
    {
        var jobName = model.Job?.Trim();
        if (jobName.IsNullOrEmpty()) return [];

        if (app == null) return [];

        // 应用停止发放作业
        app = App.FindByID(app.ID) ?? app;
        if (!app.Enable) return [];

        var job = app.Jobs.FirstOrDefault(e => e.Name == jobName);

        // 全局锁，确保单个作业只有一个线程在分配作业
        using var ck = _cacheProvider.AcquireLock($"antjob:lock:{job.ID}", 15_000);

        // 找到作业。为了确保能够快速拿到新的作业参数，这里做二次查询
        if (job != null)
            job = Job.Find(Job._.ID == job.ID);
        else
            job = Job.FindByAppIDAndName(app.ID, jobName);

        if (job == null) throw new XException($"应用[{app.ID}/{app.Name}]下未找到作业[{jobName}]");
        if (job.Step == 0 || job.DataTime.Year <= 2000) throw new XException("作业[{0}/{1}]未设置数据时间或步进", job.ID, job.Name);

        var online = _appService.GetOnline(app, ip);

        var list = new List<JobTask>();

        // 每分钟检查一下错误任务和中断任务
        CheckOldTask(app, job, model.Count, list, ip);

        // 错误项不够时，增加切片
        if (list.Count < model.Count)
        {
            //var ps = ControllerContext.Current.Parameters;
            var server = online.Name;
            var pid = online.ProcessId;
            //var topic = ps["topic"] + "";

            switch (job.Mode)
            {
                case JobModes.Message:
                    list.AddRange(AcquireMessage(job, model.Topic, server, ip, pid, model.Count - list.Count, _cacheProvider.Cache));
                    break;
                case JobModes.Time:
                    {
                        // 如果能够切片，则查询数据库后进入，避免缓存导致重复
                        if (TrySplitTime(job, false, out var end))
                        {
                            // 申请任务前，不能再查数据库，那样子会导致多线程脏读，从而出现多客户端分到相同任务的情况
                            //jb = Job.FindByKey(jb.ID);
                            list.AddRange(Acquire(job, server, ip, pid, model.Count - list.Count, _cacheProvider.Cache));
                        }
                        break;
                    }
                case JobModes.Data:
                //case JobModes.CSharp:
                //case JobModes.Sql:
                default:
                    {
                        // 如果能够切片，则查询数据库后进入，避免缓存导致重复
                        if (TrySplit(job, job.DataTime, job.Step, out var end))
                        {
                            // 申请任务前，不能再查数据库，那样子会导致多线程脏读，从而出现多客户端分到相同任务的情况
                            //jb = Job.FindByKey(jb.ID);
                            list.AddRange(Acquire(job, server, ip, pid, model.Count - list.Count, _cacheProvider.Cache));
                        }
                    }
                    break;
            }
        }

        // 记录状态
        online.Tasks += list.Count;
        online.SaveAsync();

        //return list.Select(e => e.ToModel()).ToArray();

        // 服务端下发的时间，约定UTC
        var rs = new List<ITask>();
        foreach (var task in list)
        {
            var model2 = task.ToModel();
            model2.DataTime = model2.DataTime.ToUniversalTime();
            if (model2.End.Year > 1000)
                model2.End = model2.End.ToUniversalTime();

            rs.Add(model2);
        }

        return rs.ToArray();
    }

    private void CheckOldTask(App app, Job job, Int32 count, List<JobTask> list, String ip)
    {
        // 每分钟检查一下错误任务和中断任务
        var nextKey = $"antjob:NextAcquireOld_{job.ID}";
        var now = TimerX.Now;
        var next = _cacheProvider.Cache.Get<DateTime>(nextKey);
        if (next < now)
        {
            var online = _appService.GetOnline(app, ip);

            next = now.AddSeconds(60);
            list.AddRange(AcquireOld(job, online.Server, ip, online.ProcessId, count, _cacheProvider.Cache));

            if (list.Count > 0)
            {
                // 既然有数据，待会还来
                next = now;

                var n1 = list.Count(e => e.Status is JobStatus.错误 or JobStatus.取消);
                var n2 = list.Count(e => e.Status is JobStatus.就绪 or JobStatus.抽取中 or JobStatus.处理中);
                var n3 = list.Count(e => e.Status == JobStatus.延迟);
                _log.Info("作业[{0}/{1}]准备处理[{2}]个错误、[{3}]超时和[{4}]个延迟任务 [{5}]", app, job.Name, n1, n2, n3, list.Join(",", e => e.ID + ""));
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
    /// <param name="result"></param>
    /// <returns></returns>
    public Boolean Report(App app, TaskResult result, String ip)
    {
        if (result == null || result.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + result?.ID);

        // 判断是否有权

        var task = JobTask.FindByID(result.ID) ?? throw new InvalidOperationException($"找不到任务[{result.ID}]");
        var job = Job.FindByID(task.JobID);
        if (job == null || job.AppID != app.ID)
        {
            _log.Info(result.ToJson());
            throw new InvalidOperationException($"应用[{app}]无权操作作业[{job}#{task}]");
        }

        // 只有部分字段允许客户端修改
        if (result.Status > 0) task.Status = result.Status;

        task.Speed = result.Speed;
        task.Total = result.Total;
        task.Success = result.Success;
        task.Cost = result.Cost;
        task.Key = result.Key;
        task.Message = result.Message;

        //var traceId = result.TraceId ?? DefaultSpan.Current + "";
        //if (!traceId.IsNullOrEmpty()) task.TraceId = traceId;
        //task.TraceId += $",{result.TraceId},{DefaultSpan.Current}";
        var tis = task.TraceId.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        var traceId = result.TraceId;
        if (!traceId.IsNullOrEmpty())
        {
            var ss = traceId.Split('-');
            if (ss.Length > 3 && ss[0].Length == 2) traceId = ss[1];
            if (!tis.Contains(traceId)) tis.Add(traceId);
        }
        traceId = DefaultSpan.Current?.TraceId;
        if (!traceId.IsNullOrEmpty() && !tis.Contains(traceId)) tis.Add(traceId);
        task.TraceId = tis.Join(",");
        while (true)
        {
            if (task.TraceId.Length <= JobTask._.TraceId.Length) break;
            tis.RemoveAt(0);
            task.TraceId = tis.Join(",");
        }
        // 已终结的任务，汇总统计
        if (result.Status is JobStatus.完成 or JobStatus.错误)
        {
            task.Times++;

            SetJobFinish(job, task);

            // 记录状态
            _appService.UpdateOnline(app, task, ip);
        }
        else if (result.Status == JobStatus.错误)
        {
            SetJobError(job, task);

            task.Error++;
            //ji.Message = err.Message;

            // 出错时判断如果超过最大错误数，则停止作业
            CheckMaxError(app, job);
        }
        else if (result.Status == JobStatus.延迟)
        {
            using var span = _tracer?.NewSpan("Delay", new { job.Name, task.DataTime, NextTime = result.NextTime.ToLocalTime() });

            task.Times++;

            // 延迟任务的下一次执行时间
            if (result.NextTime.Year > 2000)
                task.UpdateTime = result.NextTime.ToLocalTime();
        }

        // 从创建到完成的全部耗时
        var ts = DateTime.Now - task.CreateTime;
        task.FullCost = (Int32)ts.TotalSeconds;

        task.Update();

        return true;
    }

    private void SetJobFinish(Job job, JobTask task)
    {
        using var span = _tracer?.NewSpan(nameof(SetJobFinish), new { job.Name, task.DataTime });

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
        using var span = _tracer?.NewSpan(nameof(SetJobError), new { job.Name, task.DataTime });

        var err = new JobError
        {
            AppID = job.AppID,
            JobID = job.ID,
            TaskID = task.ID,
            DataTime = task.DataTime,
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

    #region 申请任务
    /// <summary>用于表示切片批次的序号</summary>
    private Int32 _idxBatch;

    /// <summary>申请任务分片</summary>
    /// <param name="server">申请任务的服务端</param>
    /// <param name="ip">申请任务的IP</param>
    /// <param name="pid">申请任务的服务端进程ID</param>
    /// <param name="count">要申请的任务个数</param>
    /// <param name="cache">缓存对象</param>
    /// <returns></returns>
    public IList<JobTask> Acquire(Job job, String server, String ip, Int32 pid, Int32 count, ICache cache)
    {
        var list = new List<JobTask>();

        if (!job.Enable) return list;

        var step = job.Step;
        if (step <= 0) step = 30;

        using var span = _tracer?.NewSpan(nameof(Acquire), new { job.Name, server, ip, pid, count });

        using var ts = Job.Meta.CreateTrans();
        var start = job.DataTime;
        for (var i = 0; i < count; i++)
        {
            var end = DateTime.MinValue;
            if (job.Mode == JobModes.Time && !TrySplitTime(job, true, out end) ||
                job.Mode != JobModes.Time && !TrySplit(job, start, step, out end))
                break;

            // 创建新的任务
            var task = new JobTask
            {
                AppID = job.AppID,
                JobID = job.ID,
                DataTime = start,
                End = end,
                Data = job.Data,
                BatchSize = job.BatchSize,

                Server = server,
                ProcessID = Interlocked.Increment(ref _idxBatch),
                Client = $"{ip}@{pid}",
                Status = JobStatus.就绪,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            //// 如果有模板，则进行计算替换
            //if (!Data.IsNullOrEmpty()) ti.Data = TemplateHelper.Build(Data, ti.DataTime, ti.End);

            // 重复切片判断
            var key = $"job:task:{job.ID}:{start:yyyyMMddHHmmss}";
            if (!cache.Add(key, task, 30))
            {
                var ti2 = cache.Get<JobTask>(key);
                XTrace.WriteLine("[{0}]重复切片：{1}", key, ti2?.ToJson());
                using var span2 = DefaultTracer.Instance?.NewSpan($"job:AcquireDuplicate", ti2);
            }
            else
            {
                task.Insert();

                list.Add(task);
            }

            // 更新任务
            job.DataTime = end;
            start = end;
        }

        if (list.Count > 0)
        {
            // 任务需要ID，不能批量插入优化
            //list.Insert(null);

            job.LastStatus = JobStatus.就绪;
            job.LastTime = DateTime.Now;

            job.UpdateTime = DateTime.Now;
            job.Save();
            ts.Commit();
        }

        // 记录任务数
        span?.AppendTag(null, list.Count);

        return list;
    }

    /// <summary>尝试分割时间片</summary>
    /// <param name="job"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Boolean TrySplitTime(Job job, Boolean modify, out DateTime end)
    {
        var start = job.DataTime;

        // 当前时间减去偏移量，作为当前时间。数据抽取不许超过该时间。去掉毫秒
        var now = DateTime.Now.AddSeconds(-job.Offset).Trim("s");

        end = DateTime.MinValue;

        // 开始时间和结束时间是否越界
        if (start >= now) return false;

        // 任务结束时间超过作业结束时间时，取后者
        if (job.End.Year > 2000 && end > job.End) end = job.End;

        // 时间区间判断
        if (end.Year > 2000 && start >= end) return false;

        if (modify)
        {
            // 计算下一次执行时间
            if (!job.Cron.IsNullOrEmpty())
            {
                var cron = new Cron(job.Cron);
                var time = job.DataTime.Year > 2000 ? job.DataTime : DateTime.Now;
                end = job.DataTime = cron.GetNext(time);
            }
            else
            {
                var step = job.Step;
                if (step <= 0) step = 30;
                end = job.DataTime = job.DataTime.AddSeconds(step);
            }
        }

        return true;
    }

    /// <summary>尝试分割时间片</summary>
    /// <param name="start"></param>
    /// <param name="step"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Boolean TrySplit(Job job, DateTime start, Int32 step, out DateTime end)
    {
        // 当前时间减去偏移量，作为当前时间。数据抽取不许超过该时间
        var now = DateTime.Now.AddSeconds(-job.Offset);
        // 去掉毫秒
        now = now.Trim();

        end = DateTime.MinValue;

        // 开始时间和结束时间是否越界
        if (start >= now) return false;

        if (step <= 0) step = 30;

        // 必须严格要求按照步进大小分片，除非有合适的End
        end = start.AddSeconds(step);
        // 任务结束时间超过作业结束时间时，取后者
        if (job.End.Year > 2000 && end > job.End) end = job.End;

        // 时间片必须严格要求按照步进大小分片，除非有合适的End
        if (end > now) return false;

        // 时间区间判断
        if (start >= end) return false;

        return true;
    }

    /// <summary>申请历史错误或中断的任务</summary>
    /// <param name="server">申请任务的服务端</param>
    /// <param name="ip">申请任务的IP</param>
    /// <param name="pid">申请任务的服务端进程ID</param>
    /// <param name="count">要申请的任务个数</param>
    /// <param name="cache">缓存对象</param>
    /// <returns></returns>
    public IList<JobTask> AcquireOld(Job job, String server, String ip, Int32 pid, Int32 count, ICache cache)
    {
        using var span = _tracer?.NewSpan(nameof(AcquireOld), new { job.Name, server, ip, pid, count });

        using var ts = Job.Meta.CreateTrans();
        var list = new List<JobTask>();

        // 查找历史错误任务
        if (job.ErrorDelay > 0)
        {
            var dt = DateTime.Now.AddSeconds(-job.ErrorDelay);
            var list2 = JobTask.Search(job.ID, dt, job.MaxRetry, 32, [JobStatus.错误, JobStatus.取消, JobStatus.延迟], count);
            if (list2.Count > 0) list.AddRange(list2);
        }

        // 查找历史中断任务，持续10分钟仍然未完成
        if (job.MaxTime > 0 && list.Count < count)
        {
            var dt = DateTime.Now.AddSeconds(-job.MaxTime);
            var list2 = JobTask.Search(job.ID, dt, job.MaxRetry, 32, [JobStatus.就绪, JobStatus.抽取中, JobStatus.处理中], count - list.Count);
            if (list2.Count > 0) list.AddRange(list2);
        }
        if (list.Count > 0)
        {
            foreach (var task in list)
            {
                task.Server = server;
                task.ProcessID = Interlocked.Increment(ref _idxBatch);
                task.Client = $"{ip}@{pid}";
                task.Status = JobStatus.就绪;
                //task.CreateTime = DateTime.Now;
                task.UpdateTime = DateTime.Now;
            }
            list.Save();
        }

        ts.Commit();

        // 记录任务数
        span?.AppendTag(null, list.Count);

        return list;
    }

    /// <summary>申请任务分片</summary>
    /// <param name="topic">主题</param>
    /// <param name="server">申请任务的服务端</param>
    /// <param name="ip">申请任务的IP</param>
    /// <param name="pid">申请任务的服务端进程ID</param>
    /// <param name="count">要申请的任务个数</param>
    /// <param name="cache">缓存对象</param>
    /// <returns></returns>
    public IList<JobTask> AcquireMessage(Job job, String topic, String server, String ip, Int32 pid, Int32 count, ICache cache)
    {
        // 消费消息时，保存主题
        if (job.Topic != topic)
        {
            job.Topic = topic;
            job.SaveAsync();
        }

        var list = new List<JobTask>();

        if (!job.Enable) return list;

        // 验证消息数
        var now = DateTime.Now;
        if (job.MessageCount == 0 && job.UpdateTime.AddMinutes(2) > now) return list;

        using var span = _tracer?.NewSpan(nameof(AcquireMessage), new { job.Name, topic, server, ip, pid, count });

        using var ts = Job.Meta.CreateTrans();
        var size = job.BatchSize;
        if (size == 0) size = 1;

        // 消费消息。请求任务数量=空闲线程*批大小
        var msgs = AppMessage.GetTopic(job.AppID, topic, now, count * size);
        if (msgs.Count > 0)
        {
            for (var i = 0; i < msgs.Count;)
            {
                var msgList = msgs.Skip(i).Take(size).ToList();
                if (msgList.Count == 0) break;

                i += msgList.Count;

                // 创建新的分片
                var ti = new JobTask
                {
                    AppID = job.AppID,
                    JobID = job.ID,
                    Data = msgList.Select(e => e.Data).ToJson(),
                    MsgCount = msgList.Count,
                    BatchSize = size,

                    Server = server,
                    ProcessID = Interlocked.Increment(ref _idxBatch),
                    Client = $"{ip}@{pid}",
                    Status = JobStatus.就绪,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                ti.Insert();

                // 从去重缓存去掉
                cache.Remove(msgList.Select(e => $"antjob:{job.AppID}:{job.Topic}:{e}").ToArray());

                list.Add(ti);
            }

            // 批量删除消息
            msgs.Delete();
        }

        // 更新作业下的消息数
        job.MessageCount = AppMessage.FindCountByAppIDAndTopic(job.AppID, topic);
        job.UpdateTime = now;
        job.Save();

        // 消费完成后，更新应用的消息数
        if (job.MessageCount == 0)
        {
            var app = job.App;
            if (app != null)
            {
                app.MessageCount = AppMessage.FindCountByAppID(job.ID);
                app.SaveAsync();
            }
        }

        ts.Commit();

        // 记录任务数
        span?.AppendTag(null, list.Count);

        return list;
    }
    #endregion

    #region 辅助
    /// <summary>兼容旧数据库。如DataTime对应Start</summary>
    public static void FixOld()
    {
        using var span = DefaultTracer.Instance?.NewSpan(nameof(FixOld));

        var dal = DAL.Create("Ant");
        FixOld(dal, "Job");
        FixOld(dal, "JobTask");
        FixOld(dal, "JobError");
    }

    /// <summary>兼容旧数据库。如DataTime对应Start</summary>
    /// <param name="dal"></param>
    /// <param name="tableName"></param>
    public static void FixOld(DAL dal, String tableName)
    {
        var table = dal.Tables.FirstOrDefault(e => tableName.EqualIgnoreCase(e.Name, e.TableName));
        if (table == null) return;

        // 如果不包括Start列，则不需要处理
        if (!table.Columns.Any(e => e.Name.EqualIgnoreCase("Start"))) return;
        if (!table.Columns.Any(e => e.Name.EqualIgnoreCase("DataTime"))) return;

        // 查询最后一批数据，更新字段值
        var id = dal.Query<Int32>($"select id from {tableName} where (DataTime is null or DataTime<'2000-01-01') order by id desc", null, 0, 1).FirstOrDefault();
        if (id == 0) return;

        XTrace.WriteLine("数据表[{0}]最大Id是：{1}，开始修正", tableName, id);
        if (id > 100_000)
            id -= 100_000;
        else
            id = 0;

        var rs = dal.Execute($"update {tableName} set DataTime=Start where ID>{id} and (DataTime is null or DataTime<'2000-01-01')");
        XTrace.WriteLine("从Id={0}开始，共修正{1}行", id, rs);
    }
    #endregion
}
