using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AntJob.Data;
using NewLife;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Xml;

namespace AntJob.Providers
{
    /// <summary>文件任务提供者</summary>
    public class JobFileProvider : JobProvider
    {
        private JobFile _File;

        /// <summary>销毁</summary>
        /// <param name="disposing"></param>
        protected override void OnDispose(Boolean disposing)
        {
            base.OnDispose(disposing);

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
                    var job = item.Value.CreateInstance() as Job;
                    var df = job?.Model;
                    if (df != null) model.Copy(df);

                    if (model.Start.Year <= 2000) model.Start = DateTime.Now.Date;
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
        /// <param name="names">名称列表</param>
        /// <returns></returns>
        public override IJob[] GetJobs(String[] names)
        {
            var jf = _File = JobFile.Current;

            var list = new List<IJob>();
            if (jf.Jobs != null)
            {
                foreach (var item in jf.Jobs)
                {
                    if (names.Contains(item.Name)) list.Add(item);
                }
            }

            return list.ToArray();
        }

        /// <summary>申请任务</summary>
        /// <param name="job">作业</param>
        /// <param name="data">扩展数据</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public override ITask[] Acquire(IJob job, IDictionary<String, Object> data, Int32 count)
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

            var start = job.Start;
            for (var i = 0; i < count; i++)
            {
                // 开始时间和结束时间是否越界
                if (start >= now) break;

                var end = start.AddSeconds(step);
                // 任务结束时间超过作业结束时间时，取后者
                if (job.End.Year > 2000 && end > job.End) end = job.End;

                // 时间片必须严格要求按照步进大小分片，除非有合适的End
                if (job.Mode != JobModes.Alarm)
                {
                    if (end > now) break;
                }

                // 时间区间判断
                if (start >= end) break;

                // 切分新任务
                var set = new TaskModel
                {
                    Start = start,
                    End = end,
                    Step = job.Step,
                    Offset = job.Offset,
                    BatchSize = job.BatchSize,
                };

                // 更新任务
                job.Start = end;
                start = end;

                list.Add(set);
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
                var n = 0;
                if (set.End > set.Start) n = (Int32)(set.End - set.Start).TotalSeconds;
                var msg = $"{ctx.Job.Name} 处理{ctx.Total:n0} 行，区间（{set.Start} + {n}, {set.End:HH:mm:ss}）";
                if (ctx.Job.Mode == JobModes.Alarm)
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
            foreach (var item in typeof(Job).GetAllSubclasses(false))
            {
                if (item.IsAbstract) continue;

                var name = item.GetDisplayName();
                if (name.IsNullOrEmpty())
                {
                    var wrk = item.CreateInstance() as Job;
                    name = wrk?.Name;
                }
                if (name.IsNullOrEmpty()) name = item.FullName;

                if (dic.TryGetValue(name, out var type))
                    XTrace.WriteLine("工作者[{0}]重复出现，Type1={1}，Type2={2}", name, type.FullName, item.FullName);
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
}