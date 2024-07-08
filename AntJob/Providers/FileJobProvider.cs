using System.ComponentModel;
using AntJob.Data;
using NewLife;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Xml;

namespace AntJob.Providers;

/// <summary>文件作业提供者</summary>
public class FileJobProvider : JobProvider
{
    private JobFile _File;

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _File.TryDispose();
    }

    /// <summary>开始</summary>
    public override void Start()
    {
        var jf = _File = JobFile.Current;

        var list = new List<JobModel>();
        if (jf.Jobs != null && jf.Jobs.Length > 0) list.AddRange(jf.Jobs);

        // 扫描所有Worker并添加到作业文件
        var flag = false;
        foreach (var item in GetAll())
        {
            if (!list.Any(e => e.Name == item.Key))
            {
                // 新增作业项
                var model = new JobModel();

                // 获取默认设置
                var job = item.Value.CreateInstance() as Handler;
                var df = job?.Job;
                if (df != null) model.Copy(df);

                if (model.DataTime.Year <= 2000) model.DataTime = DateTime.Now.Date;
                if (model.Step <= 0) model.Step = 30;
                if (model.BatchSize <= 0) model.BatchSize = 10000;
                if (model.MaxTask <= 0) model.MaxTask = Environment.ProcessorCount;

                if (model.Name.IsNullOrEmpty())
                {
                    model.Name = item.Key;
                    model.Enable = true;
                }

                list.Add(model);

                flag = true;
            }
        }
        if (flag)
        {
            if (jf.Jobs == null || jf.Jobs.Length == 0) jf.CreateTime = DateTime.Now;
            jf.Jobs = list.ToArray();
        }
        jf.Save();

        base.Start();
    }

    /// <summary>获取所有作业名称</summary>
    /// <returns></returns>
    public override IJob[] GetJobs()
    {
        var jf = _File = JobFile.Current;

        var list = new List<IJob>();
        if (jf.Jobs != null)
        {
            foreach (var item in jf.Jobs)
            {
                /*if (names.Contains(item.Name))*/
                list.Add(item);
            }
        }

        return list.ToArray();
    }

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public override IJob SetJob(IJob job) => null;

    /// <summary>申请任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public override ITask[] Acquire(IJob job, String topic, Int32 count)
    {
        var list = new List<ITask>();

        if (!job.Enable) return list.ToArray();

        // 当前时间减去偏移量，作为当前时间。数据抽取不许超过该时间
        var now = DateTime.Now.AddSeconds(-job.Offset);
        // 避免毫秒级带来误差，每毫秒有10000个滴答
        var sec = now.Ticks / 1_000_0000;
        now = new DateTime(sec * 1_000_0000);

        var step = job.Step;
        if (step <= 0) step = 30;

        var start = job.DataTime;
        for (var i = 0; i < count; i++)
        {
            // 开始时间和结束时间是否越界
            if (start >= now) break;

            var end = start.AddSeconds(step);
            // 任务结束时间超过作业结束时间时，取后者
            if (job.End.Year > 2000 && end > job.End) end = job.End;

            // 时间片必须严格要求按照步进大小分片，除非有合适的End
            if (job.Mode != JobModes.Time)
            {
                if (end > now) break;
            }

            // 时间区间判断
            if (start >= end) break;

            // 切分新任务
            var task = new TaskModel
            {
                DataTime = start,
                End = end,
                //Step = job.Step,
                //Offset = job.Offset,
                BatchSize = job.BatchSize,
            };

            // 更新任务
            job.DataTime = end;
            start = end;

            list.Add(task);
        }

        if (list.Count > 0)
        {
            _File.UpdateTime = DateTime.Now;
            _File.SaveAsync();
        }

        return list.ToArray();
    }

    /// <summary>完成任务，每个任务只调用一次</summary>
    /// <param name="ctx">上下文</param>
    public override void Finish(JobContext ctx)
    {
        var ex = ctx.Error?.GetTrue();
        if (ex != null) XTrace.WriteException(ex);

        if (ctx.Total > 0)
        {
            var set = ctx.Task;
            var time = set.DataTime;
            var end = set.End;
            var n = 0;
            if (end > time) n = (Int32)(end - time).TotalSeconds;
            var msg = $"{ctx.Handler.Name} 处理{ctx.Total:n0} 行，区间（{time} + {n}, {end:HH:mm:ss}）";
            if (ctx.Handler.Mode == JobModes.Time)
                msg += $"，耗时{ctx.Cost:n0}ms";
            else
                msg += $"，速度{ctx.Speed:n0}tps，耗时{ctx.Cost:n0}ms";

            XTrace.WriteLine(msg);
        }
    }

    #region 静态扫描
    private static IDictionary<String, Type> _workers;
    /// <summary>扫描所有任务</summary>
    public static IDictionary<String, Type> GetAll()
    {
        if (_workers != null) return _workers;

        var dic = new Dictionary<String, Type>();

        // 反射所有任务
        foreach (var item in typeof(Handler).GetAllSubclasses())
        {
            if (item.IsAbstract) continue;

            var name = item.GetDisplayName();
            if (name.IsNullOrEmpty())
            {
                var wrk = item.CreateInstance() as Handler;
                name = wrk?.Name;
            }
            if (name.IsNullOrEmpty()) name = item.FullName;

            if (dic.TryGetValue(name, out var type))
                XTrace.WriteLine("处理器[{0}]重复出现，Type1={1}，Type2={2}", name, type.FullName, item.FullName);
            else
                dic[name] = item;
        }

        return _workers = dic;
    }
    #endregion
}

/// <summary>作业配置</summary>
[Description("作业配置")]
[XmlConfigFile(@"Config\Job.config", 15000)]
public class JobFile : XmlConfig<JobFile>
{
    /// <summary>创建时间</summary>
    [Description("创建时间")]
    public DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    [Description("更新时间")]
    public DateTime UpdateTime { get; set; }

    /// <summary>作业集合</summary>
    [Description("作业集合")]
    public JobModel[] Jobs { get; set; }
}