using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NewLife;
using NewLife.Log;
using NewLife.Serialization;
using NewLife.Threading;

namespace AntJob
{
    /// <summary>网络任务提供者</summary>
    public class JobNetworkProvider : JobProvider
    {
        /// <summary>调度中心地址</summary>
        public String Server { get; set; }

        /// <summary>应用编号</summary>
        public String AppID { get; set; }

        /// <summary>应用密钥</summary>
        public String Secret { get; set; }

        /// <summary>客户端</summary>
        public AntClient Ant { get; set; }

        /// <summary>开始</summary>
        public override void Start()
        {
            var svr = Server;

            // 使用配置中心账号
            var ant = new AntClient(svr)
            {
                UserName = AppID,
                Password = Secret
            };
            ant.Open();

            // 断开前一个连接
            Ant.TryDispose();
            Ant = ant;

            var ws = Schedule?.Jobs;

            var jobs = GetJobs(ws.Select(e => e.Name).ToArray());
            var list = new List<IJob>();
            foreach (var wrk in ws)
            {
                var job = wrk.Model ?? new MyJob { Name = wrk.Name };

                // 调度模式
                if (wrk != null) job.Mode = wrk.Mode;

                // 描述
                if (job is MyJob job2)
                {
                    var dis = wrk.GetType().GetDisplayName();
                    if (!dis.IsNullOrEmpty()) job2.DisplayName = dis;
                    var des = wrk.GetType().GetDescription();
                    if (!des.IsNullOrEmpty()) job2.Description = des;
                }

                list.Add(job);
            }
            if (list.Count > 0) Ant.AddJobs(list.ToArray());
        }

        private IJob[] _jobs;
        private DateTime _NextGetJobs;
        /// <summary>获取所有作业名称</summary>
        /// <param name="names">名称列表</param>
        /// <returns></returns>
        public override IJob[] GetJobs(String[] names)
        {
            // 周期性获取，避免请求过快
            var now = TimerX.Now;
            if (_jobs == null || _NextGetJobs <= now)
            {
                _NextGetJobs = now.AddSeconds(5);

                _jobs = Ant.GetJobs(names);
                if (_jobs != null)
                {
                    // 某个作业停止了，需要清理它残留的任务
                    foreach (var job in _jobs)
                    {
                        if (!job.Enable) Stop(job);
                    }
                }
            }

            return _jobs;
        }

        /// <summary>申请任务</summary>
        /// <param name="job">作业</param>
        /// <param name="data">扩展数据</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public override ITask[] Acquire(IJob job, IDictionary<String, Object> data, Int32 count) => Ant.Acquire(job.Name, count, data);

        /// <summary>生产消息</summary>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        public override Int32 Produce(String topic, String[] messages, MessageOption option = null)
        {
            if (topic.IsNullOrEmpty() || messages == null || messages.Length < 1) return 0;

            return Ant.Produce(topic, messages, option);
        }

        private static readonly String _MachineName = Environment.MachineName;
        private static readonly Int32 _ProcessID = Process.GetCurrentProcess().Id;

        /// <summary>报告进度，每个任务多次调用</summary>
        /// <param name="ctx">上下文</param>
        public override void Report(JobContext ctx)
        {
            // 不用上报抽取中
            if (ctx.Status == JobStatus.抽取中) return;

            if (!(ctx?.Task is MyTask ji)) return;

            // 区分抽取和处理
            ji.Status = ctx.Status;

            ji.Speed = ctx.Speed;
            ji.Total = ctx.Total;
            ji.Success = ctx.Success;

            ji.Server = _MachineName;
            ji.ProcessID = _ProcessID;

            Report(ctx.Job.Model, ji, null);
        }

        /// <summary>完成任务，每个任务只调用一次</summary>
        /// <param name="ctx">上下文</param>
        public override void Finish(JobContext ctx)
        {
            if (!(ctx?.Task is MyTask ji)) return;

            ji.Speed = ctx.Speed;
            ji.Total = ctx.Total;
            ji.Success = ctx.Success;
            ji.Times++;

            // 区分正常完成还是错误终止
            //ti.Status = ctx.Status;
            if (ctx.Error != null && !ctx.Job.IgnoreItemError)
                ji.Status = JobStatus.错误;
            else
            {
                ji.Status = JobStatus.完成;

                if (ctx["Message"] is String msg) ji.Message = msg;

                ji.Cost = (Int32)(ctx.Cost / 1000);
            }
            if (ji.Message.IsNullOrEmpty()) ji.Message = ctx.Remark;

            var key = ctx.Key;
            if (key.IsNullOrEmpty()) key = ctx["Key"] as String;
            ji.Key = key;

            Report(ctx.Job.Model, ji);
        }

        /// <summary>任务出错</summary>
        /// <param name="ctx">上下文</param>
        public override void Error(JobContext ctx)
        {
            var ji = ctx.Task as MyTask;

            var key = ctx.Key;
            var data = "";
            var errorCode = "";
            var message = "";

            // 单个实体对象出错，记录为任务错误
            var entity = ctx.Entity;
            if (entity != null) data = entity.ToJson();
            ji.Key = key;

            var ex = ctx.Error?.GetTrue();
            if (ex != null)
            {
                errorCode = ex.GetType()?.Name;
                if (errorCode != nameof(Exception)) errorCode = errorCode.TrimEnd(nameof(Exception));

                var msg = ctx.Error.GetMessage();
                if (msg.Contains("Exception:")) msg = msg.Substring("Exception:").Trim();
                message = msg;
            }
            ji.Message = message;

            if (ji.ID > 0)
            {
                ji.Error++;
                //ji.Message = ctx.Error?.GetTrue()?.Message;
                ji.Status = JobStatus.错误;
            }

            var ext = new
            {
                Data = data,
                ErrorCode = errorCode,
            };

            Report(ctx.Job.Model, ji, ext);
        }

        private void Report(IJob job, MyTask ji, Object ext = null)
        {
            try
            {
                Ant.Report(ji, ext);
            }
            catch (Exception ex)
            {
                XTrace.WriteLine("[{0}]的[{1}]状态报告失败！{2}", job, ji.Status, ex.GetTrue().Message);
            }
        }

        #region 日志
        /// <summary>日志</summary>
        public ILog Log { get; set; }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
        #endregion
    }
}