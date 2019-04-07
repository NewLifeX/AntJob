using System;
using System.Collections.Generic;
using NewLife;

namespace AntJob
{
    /// <summary>任务提供者接口</summary>
    public interface IJobProvider
    {
        /// <summary>调度器</summary>
        Schedule Schedule { get; set; }

        /// <summary>开始工作</summary>
        void Start();

        /// <summary>停止工作</summary>
        void Stop();

        /// <summary>获取所有作业。调度器定期获取以更新作业参数</summary>
        /// <param name="names">名称列表</param>
        /// <returns></returns>
        IJob[] GetJobs(String[] names);

        /// <summary>申请任务</summary>
        /// <param name="job">服务名</param>
        /// <param name="data">扩展数据</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        ITask[] Acquire(IJob job, IDictionary<String, Object> data, Int32 count = 1);

        /// <summary></summary>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        Int32 Produce(String topic, String[] messages, MessageOption option);

        /// <summary>停止指定作业</summary>
        /// <param name="job"></param>
        void Stop(IJob job);

        /// <summary>报告进度</summary>
        /// <param name="ctx">上下文</param>
        void Report(JobContext ctx);

        /// <summary>完成任务</summary>
        /// <param name="ctx">上下文</param>
        void Finish(JobContext ctx);

        /// <summary>任务出错</summary>
        /// <param name="ctx">上下文</param>
        void Error(JobContext ctx);
    }

    /// <summary>任务提供者基类</summary>
    public abstract class JobProvider : DisposeBase, IJobProvider
    {
        /// <summary>调度器</summary>
        public Schedule Schedule { get; set; }

        /// <summary>开始工作</summary>
        public virtual void Start() { }

        /// <summary>停止工作</summary>
        public virtual void Stop() { }

        /// <summary>获取所有作业名称</summary>
        /// <returns></returns>
        public abstract IJob[] GetJobs(String[] names);

        /// <summary>申请任务</summary>
        /// <param name="job">作业</param>
        /// <param name="data">扩展数据</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public abstract ITask[] Acquire(IJob job, IDictionary<String, Object> data, Int32 count);

        /// <summary>生产消息</summary>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        public virtual Int32 Produce(String topic, String[] messages, MessageOption option = null) => 0;

        /// <summary>停止指定作业</summary>
        /// <param name="job"></param>
        public virtual void Stop(IJob job) { }

        /// <summary>报告进度，每个任务多次调用</summary>
        /// <param name="ctx">上下文</param>
        public virtual void Report(JobContext ctx) { }

        /// <summary>完成任务，每个任务只调用一次</summary>
        /// <param name="ctx">上下文</param>
        public virtual void Finish(JobContext ctx) { }

        /// <summary>任务出错，每个任务多次调用</summary>
        /// <param name="ctx">上下文</param>
        public virtual void Error(JobContext ctx) { }
    }
}