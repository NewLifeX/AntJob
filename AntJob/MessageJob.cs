using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewLife.Serialization;

namespace AntJob
{
    /// <summary>消息调度基类</summary>
    /// <typeparam name="TModel">消息模型类</typeparam>
    public abstract class MessageJob<TModel> : Job
    {
        #region 属性
        /// <summary>主题。设置后使用消费调度模式</summary>
        public String Topic { get; set; }
        #endregion

        #region 构造
        /// <summary>实例化</summary>
        public MessageJob()
        {
            Mode = JobModes.Message;
        }
        #endregion

        #region 方法
        /// <summary>参数检查</summary>
        /// <returns></returns>
        public override Boolean Start()
        {
            if (Topic.IsNullOrEmpty()) throw new ArgumentNullException(nameof(Topic), "消息调度要求设置主题");

            return base.Start();
        }

        /// <summary>申请任务</summary>
        /// <remarks>
        /// 业务应用根据使用场景，可重载Acquire并返回空来阻止创建新任务
        /// </remarks>
        /// <param name="data">扩展数据。服务器、进程等信息</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public override ITask[] Acquire(IDictionary<String, Object> data, Int32 count = 1)
        {
            // 消费模式，设置Topic值
            if (!Topic.IsNullOrEmpty()) data[nameof(Topic)] = Topic;

            return base.Acquire(data, count);
        }

        /// <summary>解析出来一批消息</summary>
        /// <param name="ctx"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        protected override Object Fetch(JobContext ctx, ITask set)
        {
            // 只有蚂蚁调度支持消息
            if (!(set is MyTask ji)) return null;

            var ss = ji.Data.ToJsonEntity<String[]>();
            if (ss == null || ss.Length == 0) return null;

            // 消息工作者特殊优待字符串，不需要再次Json解码
            if (typeof(TModel) == typeof(String)) return ss;

            return ss.Select(e => e.ToJsonEntity<TModel>()).ToArray();
        }

        /// <summary>处理一批数据</summary>
        /// <param name="ctx">上下文</param>
        /// <returns></returns>
        protected override Int32 Execute(JobContext ctx)
        {
            var count = 0;
            foreach (var item in ctx.Data as IEnumerable)
            {
                try
                {
                    //ctx.Key = item as String;
                    ctx.Entity = item;

                    if (ProcessItem(ctx, (TModel)item)) count++;
                }
                catch (Exception ex)
                {
                    ctx.Error = ex;
                    if (!OnError(ctx)) throw;
                }
            }

            return count;
        }

        /// <summary>处理一个数据对象</summary>
        /// <param name="ctx">上下文</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        protected virtual Boolean ProcessItem(JobContext ctx, TModel message) => true;
        #endregion
    }
}