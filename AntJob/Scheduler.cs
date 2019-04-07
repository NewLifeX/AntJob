using System;
using System.Collections.Generic;
using System.Linq;
using NewLife;
using NewLife.Log;
using NewLife.Threading;

namespace AntJob
{
    /// <summary>作业调度器</summary>
    public class Scheduler : DisposeBase
    {
        #region 属性
        /// <summary>调试开关。打开内部工作者日志</summary>
        public Boolean Debug { get; set; }

        /// <summary>作业集合</summary>
        public List<Job> Jobs { get; } = new List<Job>();

        /// <summary>任务提供者</summary>
        public IJobProvider Provider { get; set; }
        #endregion

        #region 构造
        /// <summary>销毁</summary>
        /// <param name="disposing"></param>
        protected override void OnDispose(Boolean disposing)
        {
            base.OnDispose(disposing);

            Stop();
        }
        #endregion

        #region 核心方法
        /// <summary>开始</summary>
        public void Start()
        {
            // 如果没有指定工作者，则全局扫描
            var bs = Jobs.ToList();
            if (bs.Count == 0)
            {
                XTrace.WriteLine("没有可用工作者");
                return;
            }

            // 启动作业提供者，获取所有作业
            var prv = Provider;
            if (prv == null) prv = Provider = new JobFileProvider();
            if (prv.Schedule == null) prv.Schedule = this;
            prv.Start();

            var jobs = prv.GetJobs(bs.Select(e => e.Name).ToArray());
            if (jobs == null || jobs.Length == 0)
            {
                XTrace.WriteLine("没有可用作业");
                return;
            }

            // 输出日志
            var mode = $"定时{Period}秒";
            var msg = $"启动任务调度引擎[{prv}]，作业[{bs.Count}]项，模式：{mode}";
            XTrace.WriteLine(msg);

            // 设置日志
            foreach (var wrk in bs)
            {
                wrk.Schedule = this;
                wrk.Provider = prv;

                var job = wrk.Model = jobs.FirstOrDefault(e => e.Name == wrk.Name);
                if (job != null && job.Mode == 0) job.Mode = wrk.Mode;

                wrk.Log = XTrace.Log;
                wrk.Start();
            }

            // 全部启动后再加入集合
            Jobs.Clear();
            Jobs.AddRange(bs);

            // 定时执行
            if (Period > 0) _timer = new TimerX(Loop, null, 100, Period * 1000, "Job") { Async = true };
        }

        /// <summary>停止</summary>
        public void Stop()
        {
            _timer.TryDispose();
            _timer = null;

            Provider?.Stop();

            foreach (var wrk in Jobs)
            {
                wrk.Stop();
            }
        }

        /// <summary>任务调度</summary>
        /// <returns></returns>
        public Boolean Process()
        {
            var prv = Provider;
            var extend = new Dictionary<String, Object>
            {
                ["server"] = Environment.MachineName,
                ["pid"] = System.Diagnostics.Process.GetCurrentProcess().Id
            };

            // 查询所有工作者和被依赖的作业
            var ws = Jobs;
            var names = ws.Select(e => e.Name).ToList();
            names = names.Distinct().ToList();

            // 拿到工作者对应的作业
            var jobs = prv.GetJobs(names.ToArray());
            if (jobs == null) return false;

            var flag = false;
            // 遍历工作者，给空闲的增加任务
            foreach (var wrk in ws)
            {
                var job = jobs.FirstOrDefault(e => e.Name == wrk.Name);
                // 找不到或者已停用
                if (job == null || !job.Enable)
                {
                    if (wrk.Active) wrk.Stop();
                    continue;
                }

                // 可能外部添加的Worker并不完整
                wrk.Schedule = this;
                wrk.Provider = prv;

                // 更新作业参数，并启动工作者
                wrk.Model = job;
                if (job.Mode == 0) job.Mode = wrk.Mode;
                if (!wrk.Active) wrk.Start();

                // 如果正在处理任务数没达到最大并行度，则继续安排任务
                if (wrk.Busy < job.MaxTask)
                {
                    // 循环申请任务，喂饱工作者
                    var count = job.MaxTask - wrk.Busy;
                    var ts = wrk.Acquire(extend, count);

                    // 送给工作者处理
                    for (var i = 0; i < count && ts != null && i < ts.Length; i++)
                    {
                        // 准备就绪，增加Busy，避免超额分配
                        wrk.Prepare(ts[i]);

                        // 使用线程池调度，避免Task排队影响使用
                        ThreadPoolX.QueueUserWorkItem(wrk.Process, ts[i]);
                    }

                    if (ts != null && ts.Length > 0) flag = true;
                }
            }

            return flag;
        }

        /// <summary>已完成</summary>
        /// <param name="ctx"></param>
        internal protected virtual void OnFinish(JobContext ctx) => _timer?.SetNext(-1);
        #endregion

        #region 定时调度
        /// <summary>定时轮询周期。默认5秒</summary>
        public Int32 Period { get; set; } = 5;

        private TimerX _timer;
        void Loop(Object state)
        {
            // 任务调度
            var rs = Process();

            // 如果有数据，马上开始下一轮
            if (rs) TimerX.Current.SetNext(-1);
        }
        #endregion
    }
}