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

        /// <summary>作业模型。启动前作为创建作业的默认值，启动后表示作业当前设置和状态</summary>
        public IJob Model { get; set; }

        /// <summary>是否工作中</summary>
        public Boolean Active { get; private set; }

        /// <summary>调度模式</summary>
        public virtual JobModes Mode { get; set; } = JobModes.Alarm;

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
            var job = new MyJob
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
        public virtual ITask[] Acquire(IDictionary<String, Object> data, Int32 count = 1)
        {
            var prv = Provider;
            var job = Model;

            // 循环申请任务，喂饱工作者
            return prv.Acquire(job, data, count);
        }
        #endregion

        #region 整体调度
        internal void Prepare(ITask task) => Interlocked.Increment(ref _Busy);

        /// <summary>异步处理一项新任务</summary>
        /// <param name="task"></param>
        public void Process(ITask task)
        {
            if (task == null) return;

            var ctx = new JobContext
            {
                Job = this,
                Task = task,
            };

            var sw = Stopwatch.StartNew();
            try
            {
                OnProcess(ctx);
            }
            catch (Exception ex)
            {
                ctx.Error = ex;

                XTrace.WriteException(ex);
            }
            finally
            {
                Interlocked.Decrement(ref _Busy);
            }

            sw.Stop();
            ctx.Cost = sw.Elapsed.TotalMilliseconds;

            OnFinish(ctx);

            ctx.Items.Clear();
        }

        /// <summary>处理任务。内部分批处理</summary>
        /// <param name="ctx"></param>
        protected virtual void OnProcess(JobContext ctx) => ctx.Success = Execute(ctx);
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
        #endregion
    }
}