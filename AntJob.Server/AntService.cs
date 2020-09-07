using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AntJob.Data;
using AntJob.Data.Entity;
using AntJob.Models;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Net;
using NewLife.Remoting;
using NewLife.Security;
using NewLife.Serialization;
using NewLife.Threading;
using XCode;

namespace AntJob.Server
{
    [Api(null)]
    class AntService : IApi, IActionFilter
    {
        #region 属性
        /// <summary>本地节点</summary>
        public static EndPoint Local { get; set; }
        #endregion

        #region 登录
        public IApiSession Session { get; set; }

        private App _App;
        private INetSession _Net;

        /// <summary>应用登录</summary>
        /// <param name="model">模型</param>
        /// <returns></returns>
        [Api(nameof(Login))]
        public LoginResponse Login(LoginModel model)
        {
            if (model.User.IsNullOrEmpty()) throw new ArgumentNullException(nameof(model.User));

            WriteLog("[{0}]从[{1}]登录[{2}@{3}]", model.User, _Net.Remote, model.Machine, model.ProcessId);

            // 找应用
            var autoReg = false;
            var app = App.FindByName(model.User);
            if (app == null || app.Secret.MD5() != model.Pass)
            {
                app = CheckApp(app, model.User, model.Pass, _Net.Remote.Host);
                if (app == null) throw new ArgumentOutOfRangeException(nameof(model.User));

                autoReg = true;
            }

            if (app == null) throw new Exception($"应用[{model.User}]不存在！");
            if (!app.Enable) throw new Exception("已禁用！");

            // 核对密码
            if (!autoReg && !app.Secret.IsNullOrEmpty())
            {
                var pass2 = app.Secret.MD5();
                if (model.Pass != pass2) throw new Exception("密码错误！");
            }

            // 版本和编译时间
            if (app.Version.IsNullOrEmpty() || app.Version.CompareTo(model.Version) < 0) app.Version = model.Version;
            if (app.CompileTime < model.Compile) app.CompileTime = model.Compile;
            if (app.DisplayName.IsNullOrEmpty()) app.DisplayName = model.DisplayName;

            app.Save();

            // 应用上线
            var online = CreateOnline(app, _Net, model.Machine, model.ProcessId);
            online.Version = model.Version;
            online.CompileTime = model.Compile;
            online.Save();

            // 记录当前用户
            Session["App"] = app;

            WriteHistory(autoReg ? "注册" : "登录", true, $"[{model.User}/{model.Pass}]在[{model.Machine}@{model.ProcessId}]登录[{app}]成功");

            var rs = new LoginResponse { Name = app.Name, DisplayName = app.DisplayName };
            if (autoReg) rs.Secret = app.Secret;

            return rs;
        }

        protected virtual App CheckApp(App app, String user, String pass, String ip)
        {
            //  本地账号不存在时
            var name = user;
            if (app == null)
            {
                // 是否支持自动注册
                var set = Setting.Current;
                if (!set.AutoRegistry) throw new Exception($"找不到应用[{name}]");

                app = new App();
                app.Secret = Rand.NextString(16);
            }
            else if (app.Secret.MD5() != pass)
            {
                // 是否支持自动注册
                var set = Setting.Current;
                if (!set.AutoRegistry) throw new Exception($"应用[{name}]申请重新激活，但服务器设置禁止自动注册");

                if (app.Secret.IsNullOrEmpty()) app.Secret = Rand.NextString(16);
            }

            if (app.ID == 0)
            {
                app.Name = name;
                app.CreateIP = ip;
                app.CreateTime = DateTime.Now;

                // 首次打开
                app.Enable = true;
            }

            app.UpdateIP = ip;
            app.UpdateTime = DateTime.Now;

            //app.Save();

            return app;
        }

        void IActionFilter.OnActionExecuting(ControllerContext filterContext)
        {
            var act = filterContext.ActionName;
            if (act == nameof(Login)) return;

            _Net = Session as INetSession;
            if (Session["App"] is App app)
            {
                _App = app;

                var online = GetOnline(app, _Net);
                online.UpdateTime = TimerX.Now;
                online.SaveAsync();
            }
            else
            {
                throw new ApiException(401, "{0}未登录！不能执行{1}".F(_Net.Remote, act));
            }
        }

        void IActionFilter.OnActionExecuted(ControllerContext filterContext)
        {
            if (filterContext.Exception != null && !filterContext.ExceptionHandled)
            {
                // 显示错误
                var ex = filterContext.Exception;
                if (ex != null)
                {
                    if (ex is ApiException)
                        XTrace.Log.Error(ex.Message);
                    else
                        XTrace.WriteException(ex);

                    WriteHistory(filterContext.ActionName, false, ex.GetMessage());
                }
            }
        }
        #endregion

        #region 业务
        /// <summary>获取指定名称的作业</summary>
        /// <returns></returns>
        [Api(nameof(GetJobs))]
        public IJob[] GetJobs()
        {
            var jobs = Job.FindAllByAppID(_App.ID);

            return jobs.Select(e => e.ToModel()).ToArray();
        }

        /// <summary>批量添加作业</summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        [Api(nameof(AddJobs))]
        public String[] AddJobs(JobModel[] jobs)
        {
            if (jobs == null || jobs.Length == 0) return new String[0];

            var myJobs = Job.FindAllByAppID(_App.ID);
            var list = new List<String>();
            foreach (var item in jobs)
            {
                var jb = myJobs.FirstOrDefault(e => e.Name.EqualIgnoreCase(item.Name));
                if (jb == null)
                {
                    jb = new Job
                    {
                        AppID = _App.ID,
                        Name = item.Name,
                        //Enable = item.Enable,
                        Start = item.Start,
                        End = item.End,
                        Offset = item.Offset,
                        Step = item.Step,
                        BatchSize = item.BatchSize,
                        MaxTask = item.MaxTask,
                        Mode = item.Mode,
                        MaxError = 100,
                    };
                }

                if (item.Mode > 0) jb.Mode = item.Mode;
                if (!item.DisplayName.IsNullOrEmpty()) jb.DisplayName = item.DisplayName;
                if (!item.Description.IsNullOrEmpty()) jb.Remark = item.Description;
                if (!item.ClassName.IsNullOrEmpty()) jb.ClassName = item.ClassName;

                if (jb.Save() != 0)
                {
                    WriteLog("[{0}]更新作业[{1}] @[{2}]", _App, item.Name, _Net.Remote);

                    // 更新作业数
                    jb.SaveAsync();

                    list.Add(jb.Name);
                }
            }

            return list.ToArray();
        }

        /// <summary>申请作业任务</summary>
        /// <param name="job">作业名称</param>
        /// <param name="count">任务个数</param>
        /// <param name="topic">主题</param>
        /// <returns></returns>
        [Api(nameof(Acquire))]
        public ITask[] Acquire(String job, Int32 count, String topic)
        {
            job = job?.Trim();
            if (job.IsNullOrEmpty()) return new TaskModel[0];

            var app = _App;
            if (app == null) return new TaskModel[0];

            // 应用停止发放作业
            app = App.FindByID(app.ID) ?? app;
            if (!app.Enable) return new TaskModel[0];

            // 找到作业。为了确保能够快速拿到新的作业参数，这里做二次查询
            var jb = app.Jobs.FirstOrDefault(e => e.Name == job);
            if (jb != null)
                jb = Job.FindByID(jb.ID);
            else
                jb = Job.FindByAppIDAndName(app.ID, job);

            if (jb == null) throw new XException($"应用[{app.ID}/{app.Name}]下未找到作业[{job}]");
            if (jb.Step == 0 || jb.Start.Year <= 2000) throw new XException("作业[{0}/{1}]未设置开始时间或步进", jb.ID, jb.Name);

            var online = GetOnline(app, _Net);

            var list = new List<JobTask>();

            // 每分钟检查一下错误任务和中断任务
            CheckErrorTask(app, jb, count, list);

            // 错误项不够时，增加切片
            if (list.Count < count)
            {
                //var ps = ControllerContext.Current.Parameters;
                var server = online.Name;
                var pid = online.ProcessId;
                //var topic = ps["topic"] + "";
                var ip = _Net.Remote.Host;

                switch (jb.Mode)
                {
                    case JobModes.Message:
                        list.AddRange(jb.AcquireMessage(topic, server, ip, pid, count - list.Count));
                        break;
                    case JobModes.Data:
                    case JobModes.Alarm:
                    //case JobModes.CSharp:
                    //case JobModes.Sql:
                    default:
                        {
                            // 如果能够切片，则查询数据库后进入，避免缓存导致重复
                            if (jb.TrySplit(jb.Start, jb.Step, out var end))
                            {
                                // 申请任务前，不能再查数据库，那样子会导致多线程脏读，从而出现多客户端分到相同任务的情况
                                //jb = Job.FindByKey(jb.ID);
                                list.AddRange(jb.Acquire(server, ip, pid, count - list.Count));
                            }
                        }
                        break;
                }
            }

            // 记录状态
            online.Tasks += list.Count;
            online.SaveAsync();

            return list.Select(e => e.ToModel()).ToArray();
        }

        private void CheckErrorTask(App app, Job jb, Int32 count, List<JobTask> list)
        {
            // 每分钟检查一下错误任务和中断任务
            var nextKey = $"_NextAcquireOld_{jb.ID}";
            var now = TimerX.Now;
            var ext = Session as IExtend;
            var next = (DateTime)(ext[nextKey] ?? DateTime.MinValue);
            if (next < now)
            {
                var ps = ControllerContext.Current.Parameters;
                var server = ps["server"] + "";
                var pid = ps["pid"].ToInt();
                var ip = _Net.Remote.Host;

                next = now.AddSeconds(60);
                list.AddRange(jb.AcquireOld(server, ip, pid, count));

                if (list.Count > 0)
                {
                    // 既然有数据，待会还来
                    next = now;

                    var n1 = list.Count(e => e.Status == JobStatus.错误 || e.Status == JobStatus.取消);
                    var n2 = list.Count(e => e.Status == JobStatus.就绪 || e.Status == JobStatus.抽取中 || e.Status == JobStatus.处理中);
                    XTrace.WriteLine("作业[{0}/{1}]准备处理[{2}]个错误和[{3}]超时任务 [{4}]", app, jb.Name, n1, n2, list.Join(",", e => e.ID + ""));
                }
                else
                    ext[nextKey] = next;
            }
        }

        /// <summary>生产消息</summary>
        /// <param name="job">作业</param>
        /// <param name="topic">主体</param>
        /// <param name="messages">消息集合</param>
        /// <param name="delayTime">延迟执行间隔（实际执行时间=延迟+生产时间），单位秒</param>
        /// <param name="unique">消息去重。避免单个消息被重复生产</param>
        /// <returns></returns>
        [Api(nameof(Produce))]
        public Int32 Produce(String job, String topic, String[] messages, Int32 delayTime = 0, Boolean unique = false)
        {
            if (messages == null) return 0;
            messages = messages.Distinct().ToArray();
            if (messages.Length == 0) return 0;

            var app = _App;

            // 去重过滤
            if (unique)
            {
                messages = AppMessage.Filter(app.ID, topic, messages);
                if (messages.Length == 0) return 0;
            }

            var ms = new List<AppMessage>();

            var total = 0;
            var now = DateTime.Now;
            // 延迟需要基于任务开始时间，而不能用使用当前时间，防止回头跑数据时无法快速执行
            var dTime = delayTime.ToDateTime();
            if (dTime.Year < 2000)
                dTime = now.AddSeconds(delayTime);

            var jb = Job.FindByAppIDAndName(app.ID, job);

            foreach (var item in messages)
            {
                var jm = new AppMessage
                {
                    AppID = app.ID,
                    JobID = jb == null ? 0 : jb.ID,
                    Topic = topic,
                    Data = item,
                };

                jm.CreateTime = jm.UpdateTime = now;
                if (delayTime > 0) jm.UpdateTime = dTime;

                ms.Add(jm);
            }

            // 记录消息积压数
            total = ms.BatchInsert();

            // 增加消息数
            if (total < 0) total = messages.Length;
            if (total > 0)
            {
                var job2 = app.Jobs?.FirstOrDefault(e => e.Topic == topic);
                if (job2 != null)
                {
                    job2.MessageCount += total;
                    job2.SaveAsync();
                }

                app.MessageCount += total;
                app.SaveAsync();
            }

            return total;
        }
        #endregion

        #region 状态报告
        /// <summary>报告状态（进度、成功、错误）</summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [Api(nameof(Report))]
        public Boolean Report(TaskResult task)
        {
            if (task == null || task.ID == 0) throw new InvalidOperationException("无效操作 TaskID=" + task?.ID);

            // 判断是否有权
            var app = _App;

            var jt = JobTask.FindByID(task.ID);
            if (jt == null) throw new InvalidOperationException($"找不到任务[{task.ID}]");

            var job = Job.FindByID(jt.JobID);
            if (job == null || job.AppID != app.ID)
            {
                XTrace.WriteLine(task.ToJson());
                throw new InvalidOperationException($"应用[{app}]无权操作作业[{job}#{jt}]");
            }

            // 只有部分字段允许客户端修改
            if (task.Status > 0) jt.Status = task.Status;

            jt.Speed = task.Speed;
            jt.Total = task.Total;
            jt.Success = task.Success;
            jt.Cost = task.Cost;
            jt.Key = task.Key;
            jt.Message = task.Message;

            // 已终结的作业，汇总统计
            if (task.Status == JobStatus.完成 || task.Status == JobStatus.错误)
            {
                jt.Times++;

                SetJobFinish(job, jt);

                // 记录状态
                UpdateOnline(app, jt, _Net);
            }
            if (task.Status == JobStatus.错误)
            {
                var ps = ControllerContext.Current.Parameters;

                SetJobError(job, jt, ps);

                jt.Error++;
                //ji.Message = err.Message;

                // 出错时判断如果超过最大错误数，则停止作业
                CheckMaxError(app, job);
            }

            // 从创建到完成的全部耗时
            var ts = DateTime.Now - jt.CreateTime;
            jt.FullCost = (Int32)ts.TotalSeconds;

            jt.SaveAsync();
            //ji.Save();

            return true;
        }

        private void SetJobFinish(Job job, JobTask task)
        {
            job.Total += task.Total;
            job.Success += task.Success;
            job.Error += task.Error;
            job.Times++;

            var ths = job.MaxTask;

            var p1 = task.Speed * ths;

            if (p1 > 0)
            {
                // 平均速度
                if (job.Speed > 0)
                    job.Speed = (Int32)((job.Speed * 3L + p1) / 4);
                else
                    job.Speed = p1;
            }

            job.SaveAsync();
            //job.Save();
        }

        private JobError SetJobError(Job job, JobTask task, IDictionary<String, Object> ps)
        {
            var err = new JobError
            {
                AppID = job.AppID,
                JobID = job.ID,
                Start = task.Start,
                End = task.End,
                BatchSize = task.BatchSize,

                Server = task.Server,
                ProcessID = task.ProcessID,
                Client = task.Client,

                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };
            err.Key = task.Key;
            err.Data = ps["Data"] + "";

            var code = ps["ErrorCode"] + "";
            if (code != nameof(Exception)) code = code.TrimEnd(nameof(Exception));
            err.ErrorCode = code;

            var msg = task.Message;
            if (!msg.IsNullOrEmpty() && msg.Contains("Exception:")) msg = msg.Substring("Exception:").Trim();
            err.Message = msg;

            err.Insert();

            return err;
        }

        private void CheckMaxError(App app, Job job)
        {
            // 出错时判断如果超过最大错误数，则停止作业
            var maxError = job.MaxError < 1 ? 100 : job.MaxError;
            if (job.Enable && job.Error > maxError)
            {
                job.MaxError = maxError;
                job.Enable = false;

                //job.SaveAsync();
                (job as IEntity).Update();
            }
        }
        #endregion

        #region 在线状态
        /// <summary>获取当前应用的所有在线实例</summary>
        /// <returns></returns>
        [Api(nameof(GetPeers))]
        public PeerModel[] GetPeers()
        {
            var olts = AppOnline.FindAllByAppID(_App.ID);

            return olts.Select(e => e.ToModel()).ToArray();
        }

        AppOnline CreateOnline(App app, INetSession ns, String machine, Int32 pid)
        {
            var ip = ns.Remote.Host;

            var online = GetOnline(app, ns);
            online.Client = $"{(ip.IsNullOrEmpty() ? machine : ip)}@{pid}";
            online.Name = machine;
            online.ProcessId = pid;
            online.UpdateIP = ip;
            //online.Version = version;

            online.Server = Local + "";
            //online.Save();

            // 真正的用户
            Session["AppOnline"] = online;

            // 下线
            ns.OnDisposed += (s, e) =>
            {
                online.Delete();
                WriteHistory("下线", true, $"[{online.Name}]登录于{online.CreateTime}，最后活跃于{online.UpdateTime}");
            };

            return online;
        }

        AppOnline GetOnline(App app, INetSession ns)
        {
            if (Session["AppOnline"] is AppOnline online) return online;

            var ip = ns.Remote.Host;
            var ins = ns.Remote.EndPoint + "";
            online = AppOnline.FindByInstance(ins) ?? new AppOnline { CreateIP = ip };
            online.AppID = app.ID;
            online.Instance = ins;

            return online;
        }

        void UpdateOnline(App app, JobTask ji, INetSession ns)
        {
            var online = GetOnline(app, ns);
            online.Total += ji.Total;
            online.Success += ji.Success;
            online.Error += ji.Error;
            online.Cost += ji.Cost;
            online.Speed = ji.Speed;
            online.LastKey = ji.Key;
            online.SaveAsync();
        }
        #endregion

        #region 写历史
        void WriteHistory(String action, Boolean success, String remark) => AppHistory.Create(_App, action, success, remark, Local + "", _Net.Remote?.Host);
        #endregion

        #region 日志
        /// <summary>日志</summary>
        public static ILog Log { get; set; }

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
        #endregion
    }
}