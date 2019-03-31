using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NewLife.Collections;
using NewLife.Log;

namespace AntJob
{
    /// <summary>作业基类</summary>
    public abstract class Job
    {
        #region 属性
        /// <summary>名称</summary>
        public String Name { get; set; }

        /// <summary>调度器</summary>
        public Schedule Schedule { get; set; }

        /// <summary>作业提供者</summary>
        public IJobProvider Provider { get; set; }

        /// <summary>作业</summary>
        public IJob Model { get; set; }

        /// <summary>是否工作中</summary>
        public Boolean Active { get; private set; }

        /// <summary>主题。设置后使用消费调度模式</summary>
        public String Topic { get; set; }

        /// <summary>调度模式</summary>
        public virtual JobModes Mode { get; set; } = JobModes.Alarm;

        /// <summary>是否忽略每一行数据的错误。大批量数据数据时可选忽略，默认false</summary>
        public Boolean IgnoreItemError { get; set; }

        /// <summary>是否支持分页。默认false按单页处理，DataWorker默认true</summary>
        protected Boolean SupportPage { get; set; }

        private volatile Int32 _Busy;
        /// <summary>正在处理中的任务数</summary>
        public Int32 Busy => _Busy;
        #endregion

        #region 索引器
        private readonly IDictionary<String, Object> _Items = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);
        /// <summary>用户数据</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Object this[String item] { get => _Items[item]; set => _Items[item] = value; }
        #endregion

        #region 构造
        /// <summary>实例化</summary>
        public Job()
        {
            Name = GetType().Name.TrimEnd(nameof(Job));

            var now = DateTime.Now;
            var job = new JobModel
            {
                Start = new DateTime(now.Year, now.Month, 1),
                Step = 30,
                Offset = 15,
            };

            // 默认并发数为核心数
            job.MaxTask = Environment.ProcessorCount;
            if (job.MaxTask > 8) job.MaxTask = 8;

            Model = job;
        }
        #endregion

        #region 基本方法
        /// <summary>开始</summary>
        public virtual Boolean Start()
        {
            if (Active) return false;

            var msg = "开始工作";
            var job = Model;
            if (job != null) msg += " {0} 区间（{1}, {2}） Offset={3} Step={4} MaxTask={5}".F(job.Enable, job.Start, job.End, job.Offset, job.Step, job.MaxTask);

            WriteLog(msg);

            Active = true;

            return true;
        }

        /// <summary>停止</summary>
        public virtual Boolean Stop()
        {
            if (!Active) return false;

            WriteLog("停止工作");

            Active = false;

            Provider?.Stop(Model);

            return true;
        }
        #endregion

        #region 申请任务
        /// <summary>申请任务</summary>
        /// <remarks>
        /// 业务应用根据使用场景，可重载Acquire并返回空来阻止创建新任务
        /// </remarks>
        /// <param name="data">扩展数据。服务器、进程等信息</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public virtual IJobItem[] Acquire(IDictionary<String, Object> data, Int32 count = 1)
        {
            var prv = Provider;
            var job = Model;
            // 消费模式，设置Topic值
            if (!Topic.IsNullOrEmpty()) data[nameof(Topic)] = Topic;

            // 循环申请任务，喂饱工作者
            return prv.Acquire(job, data, count);
        }
        #endregion

        #region 整体调度
        internal void Prepare(IJobItem task) => Interlocked.Increment(ref _Busy);

        /// <summary>异步处理一项新任务</summary>
        /// <param name="task"></param>
        public void Process(IJobItem task)
        {
            if (task == null) return;

            try
            {
                OnProcess(task);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
            finally
            {
                Interlocked.Decrement(ref _Busy);
            }
        }

        /// <summary>处理任务。内部分批处理，多次调用OnProcess</summary>
        /// <param name="set"></param>
        private void OnProcess(IJobItem set)
        {
            var ctx = new JobContext
            {
                Job = this,
                Setting = set,
            };

            var swTotal = Stopwatch.StartNew();
            var prov = Provider;
            while (true)
            {
                ctx.Data = null;
                ctx.Entity = null;
                ctx.Error = null;

                try
                {
                    // 报告进度
                    ctx.Status = JobStatus.抽取中;
                    prov?.Report(ctx);

                    var sw = Stopwatch.StartNew();

                    // 分批抽取
                    var data = Fetch(ctx, set);
                    sw.Stop();

                    var list = data as IList;
                    if (list != null) ctx.Total += list.Count;
                    ctx.FetchCost += sw.Elapsed.TotalMilliseconds;
                    ctx.Data = data;

                    // 报告进度
                    ctx.Status = JobStatus.处理中;
                    prov?.Report(ctx);

                    if (data == null || list != null && list.Count == 0)
                    {
                        ctx.Status = JobStatus.完成;
                        break;
                    }

                    sw = Stopwatch.StartNew();

                    // 批量处理
                    ctx.Success += Execute(ctx);

                    sw.Stop();
                    ctx.ProcessCost += sw.Elapsed.TotalMilliseconds;

                    // 报告进度
                    ctx.Status = JobStatus.完成;

                    // 不满一批，结束
                    if (list != null && list.Count < set.BatchSize) break;
                }
                catch (Exception ex)
                {
                    ctx.Status = JobStatus.错误;

                    ctx.Error = ex;

                    if (!OnError(ctx)) break;

                    // 抽取异常，退出任务。处理异常则继续
                    if (ctx.Data == null) break;
                }

                if (!SupportPage) break;
            }

            swTotal.Stop();
            ctx.TotalCost = swTotal.Elapsed.TotalMilliseconds;

            // 忽略内部异常，有错误已经被处理，这里不需要再次报告
            if (ctx.Error == null) OnFinish(ctx);

            ctx.Items.Clear();
        }
        #endregion

        #region 数据抽取
        /// <summary>分批抽取数据，一个任务内多次调用</summary>
        /// <param name="ctx">上下文</param>
        /// <param name="set"></param>
        /// <returns></returns>
        protected virtual Object Fetch(JobContext ctx, IJobItem set)
        {
            if (set == null) throw new ArgumentNullException(nameof(set), "没有设置数据抽取配置");

            // 时间未到
            if (set.Start > DateTime.Now) return null;

            var list = new List<DateTime>
            {
                set.Start
            };

            return list;
        }
        #endregion

        #region 数据处理
        /// <summary>处理一批数据，一个任务内多次调用</summary>
        /// <param name="ctx">上下文</param>
        /// <returns></returns>
        protected abstract Int32 Execute(JobContext ctx);

        /// <summary>生产消息</summary>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        public Int32 Produce(String topic, String[] messages, MessageOption option = null) => Provider.Produce(topic, messages, option);

        private String _lastError;
        /// <summary>遇到错误时如何处理，返回是否已处理</summary>
        /// <param name="ctx">数据上下文</param>
        /// <returns>是否处理成功</returns>
        protected virtual Boolean OnError(JobContext ctx)
        {
            var ex = ctx.Error;
            ex = ex?.GetTrue();
            if (ex == null) return true;

            Provider?.Error(ctx);

            if (ctx.Entity != null)
            {
                // 忽略子项异常
                if (IgnoreItemError) return true;
            }

            if (ShowError)
            {
                // 过滤掉重复错误日志
                if (_lastError != ex.Message)
                {
                    _lastError = ex.Message;

                    WriteError(ctx.Error.GetMessage());
                }
            }

            return false;
        }

        /// <summary>整个任务完成</summary>
        /// <param name="ctx"></param>
        protected virtual void OnFinish(JobContext ctx) => Provider?.Finish(ctx);
        #endregion

        #region 日志
        /// <summary>日志</summary>
        public ILog Log { get; set; } = Logger.Null;

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLog(String format, params Object[] args) => Log?.Info(Name + " " + format, args);

        /// <summary>显示错误日志</summary>
        public Boolean ShowError { get; set; }

        /// <summary>写错误日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public virtual void WriteError(String format, params Object[] args) => Log?.Error(Name + " " + format, args);
        #endregion
    }
}