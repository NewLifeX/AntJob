using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Serialization;
using XCode;
using XCode.Membership;

namespace AntJob.Data.Entity
{
    /// <summary>作业</summary>
    public partial class Job : EntityBase<Job>
    {
        #region 对象操作
        static Job()
        {
            // 累加字段
            var df = Meta.Factory.AdditionalFields;
            df.Add(__.Total);
            df.Add(__.Success);
            df.Add(__.Error);
            df.Add(__.Times);
            //df.Add(__.MessageCount);

            // 过滤器 UserModule、TimeModule、IPModule
            Meta.Modules.Add<UserModule>();
            Meta.Modules.Add<TimeModule>();
            Meta.Modules.Add<IPModule>();
        }

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;

            //if ((Mode == JobModes.Sql || Mode == JobModes.CSharp) && Data.IsNullOrEmpty())
            //    throw new ArgumentNullException(nameof(Data), $"{Mode}调度模式要求设置Data模板");

            // 参数默认值
            if (Step == 0) Step = 5;
            if (MaxRetain == 0) MaxRetain = 3;
            if (MaxIdle == 0) MaxIdle = GetDefaultIdle();

            if (isNew)
            {
                if (!Dirtys[nameof(MaxRetry)]) MaxRetry = 10;
                if (!Dirtys[nameof(MaxTime)]) MaxTime = 600;
                if (!Dirtys[nameof(ErrorDelay)]) ErrorDelay = 60;
                if (!Dirtys[nameof(MaxIdle)]) MaxIdle = GetDefaultIdle();
            }

            //// 截断错误信息，避免过长
            //var len = _.Remark.Length;
            //if (!Remark.IsNullOrEmpty() && len > 0 && Remark.Length > len) Remark = Remark.Substring(0, len);

            var app = App;
            if (isNew && app != null)
            {
                app.JobCount = FindCountByAppID(app.ID);
                app.SaveAsync();
            }
        }

        private Int32 GetDefaultIdle()
        {
            // 定时调度，取步进加一分钟
            if (Mode == JobModes.Alarm) return Step + 600;

            return 3600;
        }

        /// <summary>删除</summary>
        /// <returns></returns>
        protected override Int32 OnDelete()
        {
            var rs = base.OnDelete();

            var app = App;
            if (app != null)
            {
                app.JobCount = FindCountByAppID(app.ID);
                app.SaveAsync();
            }

            return rs;
        }
        #endregion

        #region 扩展属性
        /// <summary>应用</summary>
        [XmlIgnore]
        //[ScriptIgnore]
        public App App => Extends.Get(nameof(App), k => App.FindByID(AppID));

        /// <summary>应用</summary>
        [XmlIgnore]
        //[ScriptIgnore]
        [DisplayName("应用")]
        [Map(__.AppID)]
        public String AppName => App?.Name;
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static Job FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];
        }

        /// <summary>根据应用、名称查找</summary>
        /// <param name="appid">应用</param>
        /// <param name="name">名称</param>
        /// <returns>实体对象</returns>
        public static Job FindByAppIDAndName(Int32 appid, String name)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.AppID == appid && e.Name == name);

            return Find(_.AppID == appid & _.Name == name);
        }

        /// <summary>根据应用查询</summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static IList<Job> FindAllByAppID(Int32 appid)
        {
            if (appid == 0) return new List<Job>();

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.AppID == appid);

            return FindAll(_.AppID == appid);
        }

        /// <summary>
        /// 直接查库，不查缓存
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static IList<Job> FindAllByAppID2(Int32 appid)
        {
            if (appid == 0) return new List<Job>();

            return FindAll(_.AppID == appid);
        }

        /// <summary>
        /// 查询当前应用的作业数
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static Int32 FindCountByAppID(Int32 appid)
        {
            if (appid == 0) return 0;

            return (Int32)FindCount(_.AppID == appid);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="id"></param>
        /// <param name="appid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="mode"></param>
        /// <param name="key"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IEnumerable<Job> Search(Int32 id, Int32 appid, DateTime start, DateTime end, Int32 mode, String key, PageParameter p)
        {
            var exp = new WhereExpression();

            if (id > 0) exp &= _.ID == id;
            if (appid > 0) exp &= _.AppID == appid;
            if (mode > 0) exp &= _.Mode == mode;
            if (!key.IsNullOrEmpty()) exp &= _.Name.Contains(key);
            exp &= _.CreateTime.Between(start, end);

            return FindAll(exp, p);
        }
        #endregion

        #region 业务操作
        /// <summary>是否已准备就绪</summary>
        /// <returns></returns>
        public Boolean IsReady()
        {
            switch (Mode)
            {
                case JobModes.Data:
                case JobModes.Alarm:
                    return Start.Year > 2000 && Step > 0;
                case JobModes.Message:
                    return Topic.IsNullOrEmpty();
                default:
                    break;
            }

            return false;
        }

        /// <summary>重置任务，让它从新开始工作</summary>
        /// <param name="days">重置到多少天之前</param>
        /// <param name="stime">开始时间（优先级低于days）</param>
        /// <param name="etime">结束时间（优先级低于days）</param>
        public void ResetTime(Int32 days, DateTime stime, DateTime etime)
        {
            if (days < 0)
            {
                Start = DateTime.MinValue;

                if (stime > DateTime.MinValue)
                    Start = stime;
                End = etime;
            }
            else
                Start = DateTime.Now.Date.AddDays(-days);

            Save();
        }

        /// <summary>重置任务，让它从新开始工作</summary>
        public void ResetOther()
        {
            Total = 0;
            Success = 0;
            Times = 0;
            Speed = 0;
            Error = 0;

            Save();
        }

        /// <summary>删除过期</summary>
        /// <returns></returns>
        public Int32 DeleteItems()
        {
            // 每个作业保留1000行
            var count = JobTask.FindCountByJobId(ID);
            if (count <= 1000) return 0;

            var days = MaxRetain;
            if (days <= 0) days = 3;
            var last = JobTask.FindLastByJobId(ID, DateTime.Now.AddDays(-days));
            if (last == null) return 0;

            return JobTask.DeleteByID(ID, last.ID);
        }

        /// <summary>转模型类</summary>
        /// <returns></returns>
        public JobModel ToModel()
        {
            // 如果禁用，仅返回最简单的字段
            if (!Enable) return new JobModel { Name = Name, Enable = Enable };

            return new JobModel
            {
                Name = Name,
                ClassName = ClassName,
                Enable = Enable,

                Start = Start,
                End = End,
                Topic = Topic,
                Data = Data,

                Offset = Offset,
                Step = Step,
                BatchSize = BatchSize,
                MaxTask = MaxTask,

                Mode = Mode,
            };
        }
        #endregion

        #region 申请任务
        /// <summary>申请任务分片</summary>
        /// <param name="server">申请任务的服务端</param>
        /// <param name="ip">申请任务的IP</param>
        /// <param name="pid">申请任务的服务端进程ID</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public IList<JobTask> Acquire(String server, String ip, Int32 pid, Int32 count)
        {
            var list = new List<JobTask>();

            if (!Enable) return list;

            var step = Step;
            if (step <= 0) step = 30;

            lock (this)
            {
                using var ts = Meta.CreateTrans();
                var start = Start;
                for (var i = 0; i < count; i++)
                {
                    if (!TrySplit(start, step, out var end)) break;

                    // 创建新的分片
                    var ti = new JobTask
                    {
                        AppID = AppID,
                        JobID = ID,
                        Start = start,
                        End = end,
                        BatchSize = BatchSize,
                    };

                    ti.Server = server;
                    ti.ProcessID = pid;
                    ti.Client = $"{ip}@{pid}";
                    ti.Status = JobStatus.就绪;
                    ti.CreateTime = DateTime.Now;
                    ti.UpdateTime = DateTime.Now;

                    //// 如果有模板，则进行计算替换
                    //if (!Data.IsNullOrEmpty()) ti.Data = TemplateHelper.Build(Data, ti.Start, ti.End);

                    ti.Insert();

                    // 更新任务
                    Start = end;
                    start = end;

                    list.Add(ti);
                }

                if (list.Count > 0)
                {
                    // 任务需要ID，不能批量插入优化
                    //list.Insert(null);

                    UpdateTime = DateTime.Now;
                    Save();
                    ts.Commit();
                }

                return list;
            }
        }

        /// <summary>尝试分割时间片</summary>
        /// <param name="start"></param>
        /// <param name="step"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Boolean TrySplit(DateTime start, Int32 step, out DateTime end)
        {
            // 当前时间减去偏移量，作为当前时间。数据抽取不许超过该时间
            var now = DateTime.Now.AddSeconds(-Offset);
            // 去掉毫秒
            now = now.Trim();

            end = DateTime.MinValue;

            // 开始时间和结束时间是否越界
            if (start >= now) return false;

            if (step <= 0) step = 30;

            // 必须严格要求按照步进大小分片，除非有合适的End
            end = start.AddSeconds(step);
            // 任务结束时间超过作业结束时间时，取后者
            if (End.Year > 2000 && end > End) end = End;

            // 时间片必须严格要求按照步进大小分片，除非有合适的End
            if (Mode != JobModes.Alarm)
            {
                if (end > now) return false;
            }

            // 时间区间判断
            if (start >= end) return false;

            return true;
        }

        /// <summary>申请历史错误或中断的任务</summary>
        /// <param name="server">申请任务的服务端</param>
        /// <param name="ip">申请任务的IP</param>
        /// <param name="pid">申请任务的服务端进程ID</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public IList<JobTask> AcquireOld(String server, String ip, Int32 pid, Int32 count)
        {
            lock (this)
            {
                using var ts = Meta.CreateTrans();
                var list = new List<JobTask>();

                // 查找历史错误任务
                if (ErrorDelay > 0)
                {
                    var dt = DateTime.Now.AddSeconds(-ErrorDelay);
                    var list2 = JobTask.Search(ID, dt, MaxRetry, new[] { JobStatus.错误, JobStatus.取消 }, count);
                    if (list2.Count > 0) list.AddRange(list2);
                }

                // 查找历史中断任务，持续10分钟仍然未完成
                if (MaxTime > 0 && list.Count < count)
                {
                    var dt = DateTime.Now.AddSeconds(-MaxTime);
                    var list2 = JobTask.Search(ID, dt, MaxRetry, new[] { JobStatus.就绪, JobStatus.抽取中, JobStatus.处理中 }, count - list.Count);
                    if (list2.Count > 0) list.AddRange(list2);
                }
                if (list.Count > 0)
                {
                    foreach (var ti in list)
                    {
                        ti.Server = server;
                        ti.ProcessID = pid;
                        ti.Client = $"{ip}@{pid}";
                        //ti.Status = JobStatus.就绪;
                        ti.CreateTime = DateTime.Now;
                        ti.UpdateTime = DateTime.Now;
                    }
                    list.Save();
                }

                ts.Commit();

                return list;
            }
        }

        /// <summary>申请任务分片</summary>
        /// <param name="topic">主题</param>
        /// <param name="server">申请任务的服务端</param>
        /// <param name="ip">申请任务的IP</param>
        /// <param name="pid">申请任务的服务端进程ID</param>
        /// <param name="count">要申请的任务个数</param>
        /// <returns></returns>
        public IList<JobTask> AcquireMessage(String topic, String server, String ip, Int32 pid, Int32 count)
        {
            // 消费消息时，保存主题
            if (Topic != topic)
            {
                Topic = topic;
                SaveAsync();
            }

            var list = new List<JobTask>();

            if (!Enable) return list;

            // 验证消息数
            var now = DateTime.Now;
            if (MessageCount == 0 && UpdateTime.AddMinutes(2) > now) return list;

            lock (this)
            {
                using var ts = Meta.CreateTrans();
                var size = BatchSize;
                if (size == 0) size = 1;

                // 消费消息。请求任务数量=空闲线程*批大小
                var msgs = AppMessage.GetTopic(AppID, topic, now, count * size);
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
                            AppID = AppID,
                            JobID = ID,
                            Data = msgList.Select(e => e.Data).ToJson(),
                            MsgCount = msgList.Count,

                            BatchSize = size,
                        };

                        ti.Server = server;
                        ti.ProcessID = pid;
                        ti.Client = $"{ip}@{pid}";
                        ti.Status = JobStatus.就绪;
                        ti.CreateTime = DateTime.Now;
                        ti.UpdateTime = DateTime.Now;

                        ti.Insert();

                        list.Add(ti);
                    }

                    // 批量删除消息
                    msgs.Delete();
                }

                // 更新作业下的消息数
                MessageCount = AppMessage.FindCountByAppIDAndTopic(AppID, topic);
                UpdateTime = now;
                Save();

                // 消费完成后，更新应用的消息数
                if (MessageCount == 0)
                {
                    var app = App;
                    if (app != null)
                    {
                        app.MessageCount = AppMessage.FindCountByAppID(ID);
                        app.SaveAsync();
                    }
                }

                ts.Commit();

                return list;
            }
        }
        #endregion
    }
}