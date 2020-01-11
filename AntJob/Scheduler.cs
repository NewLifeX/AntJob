using System;
using System.Collections.Generic;
using System.Linq;
using AntJob.Data;
using AntJob.Providers;
using NewLife;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Threading;

namespace AntJob
{
    /// <summary>作业调度器</summary>
    public class Scheduler : DisposeBase
    {
        #region 属性
        /// <summary>调试开关。打开内部处理器日志</summary>
        public Boolean Debug { get; set; }

        /// <summary>作业集合</summary>
        public List<Handler> Jobs { get; } = new List<Handler>();

        /// <summary>任务提供者</summary>
        public IJobProvider Provider { get; set; }
        #endregion

        #region 构造
        /// <summary>销毁</summary>
        /// <param name="disposing"></param>
        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            Stop();
        }
        #endregion

        #region 核心方法
        /// <summary>开始</summary>
        public void Start()
        {
            // 如果没有指定处理器，则全局扫描
            var bs = Jobs.ToList();
            if (bs.Count == 0)
            {
                XTrace.WriteLine("没有可用处理器");
                return;
            }

            // 启动作业提供者，获取所有作业
            var prv = Provider;
            if (prv == null) prv = Provider = new FileJobProvider();
            if (prv.Schedule == null) prv.Schedule = this;
            prv.Start();

            var jobs = prv.GetJobs();
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
            foreach (var handler in bs)
            {
                handler.Schedule = this;
                handler.Provider = prv;

                var job = handler.Job = jobs.FirstOrDefault(e => e.Name == handler.Name);
                if (job != null && job.Mode == 0) job.Mode = handler.Mode;

                handler.Log = XTrace.Log;
                handler.Start();
            }

            //// 全部启动后再加入集合
            //Jobs.Clear();
            //Jobs.AddRange(bs);

            // 定时执行
            if (Period > 0) _timer = new TimerX(Loop, null, 100, Period * 1000, "Job") { Async = true };
        }

        /// <summary>停止</summary>
        public void Stop()
        {
            _timer.TryDispose();
            _timer = null;

            Provider?.Stop();

            foreach (var handler in Jobs)
            {
                handler.Stop();
            }
        }

        /// <summary>任务调度</summary>
        /// <returns></returns>
        public Boolean Process()
        {
            var prv = Provider;

            // 查询所有处理器和被依赖的作业
            var bs = Jobs;
            //var names = bs.Select(e => e.Name).ToList();
            //names = names.Distinct().ToList();

            // 拿到处理器对应的作业
            var jobs = prv.GetJobs();
            if (jobs == null) return false;

            // 运行时动态往集合里面加处理器，为了配合Sql+C#
            CheckHandlers(prv, jobs, bs);

            var flag = false;
            // 遍历处理器，给空闲的增加任务
            foreach (var handler in bs)
            {
                var job = jobs.FirstOrDefault(e => e.Name == handler.Name);
                // 找不到或者已停用
                if (job == null || !job.Enable)
                {
                    if (handler.Active) handler.Stop();
                    continue;
                }

                // 可能外部添加的Worker并不完整
                handler.Schedule = this;
                handler.Provider = prv;

                // 更新作业参数，并启动处理器
                handler.Job = job;
                if (job.Mode == 0) job.Mode = handler.Mode;
                if (!handler.Active) handler.Start();

                // 如果正在处理任务数没达到最大并行度，则继续安排任务
                if (handler.Busy < job.MaxTask)
                {
                    // 循环申请任务，喂饱处理器
                    var count = job.MaxTask - handler.Busy;
                    var ts = handler.Acquire(null, count);

                    // 送给处理器处理
                    for (var i = 0; i < count && ts != null && i < ts.Length; i++)
                    {
                        // 准备就绪，增加Busy，避免超额分配
                        handler.Prepare(ts[i]);

                        // 使用线程池调度，避免Task排队影响使用
                        ThreadPoolX.QueueUserWorkItem(handler.Process, ts[i]);
                    }

                    if (ts != null && ts.Length > 0) flag = true;
                }
            }

            return flag;
        }

        private void CheckHandlers(IJobProvider provider, IList<IJob> jobs, IList<Handler> handlers)
        {
            foreach (var job in jobs)
            {
                var handler = handlers.FirstOrDefault(e => e.Name == job.Name);
                if (handler == null && job.Enable && !job.ClassName.IsNullOrEmpty())
                {
                    XTrace.WriteLine("发现未知作业[{0}]@[{1}]", job.Name, job.ClassName);
                    try
                    {
                        // 实例化一个处理器
                        var type = Type.GetType(job.ClassName);
                        if (type != null)
                        {
                            handler = type.CreateInstance() as Handler;
                            if (handler != null)
                            {
                                XTrace.WriteLine("添加新作业[{0}]@[{1}]", job.Name, job.ClassName);

                                handler.Name = job.Name;
                                handler.Schedule = this;
                                handler.Provider = provider;
                                handler.Log = XTrace.Log;
                                handler.Start();

                                handlers.Add(handler);
                            }
                        }
                    }
                    catch { }
                }
            }
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