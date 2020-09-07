using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AntJob.Data;
using AntJob.Models;
using NewLife;
using NewLife.Log;
using NewLife.Net;
using NewLife.Reflection;
using NewLife.Remoting;
using NewLife.Serialization;

namespace AntJob.Providers
{
    /// <summary>蚂蚁客户端</summary>
    public class AntClient : ApiClient
    {
        #region 属性
        /// <summary>用户名</summary>
        public String UserName { get; set; }

        /// <summary>密码</summary>
        public String Password { get; set; }

        /// <summary>是否已登录</summary>
        public Boolean Logined { get; set; }

        /// <summary>最后一次登录成功后的消息</summary>
        public LoginResponse Info { get; private set; }
        #endregion

        #region 方法
        /// <summary>实例化</summary>
        public AntClient()
        {
            Log = XTrace.Log;

            //StatPeriod = 60;
            ShowError = true;

#if DEBUG
            EncoderLog = XTrace.Log;
            StatPeriod = 10;
#endif
        }

        /// <summary>实例化</summary>
        /// <param name="uri"></param>
        public AntClient(String uri) : this()
        {
            if (!uri.IsNullOrEmpty())
            {
                var ss = uri.Split(",", ";");

                Servers = ss;

                var u = new Uri(ss[0]);
                var us = u.UserInfo.Split(":");
                if (us.Length > 0) UserName = us[0];
                if (us.Length > 1) Password = us[1];
            }
        }
        #endregion

        #region 登录
        /// <summary>连接后自动登录</summary>
        /// <param name="client">客户端</param>
        /// <param name="force">强制登录</param>
        protected override async Task<Object> OnLoginAsync(ISocketClient client, Boolean force)
        {
            if (Logined && !force) return null;

            var asmx = AssemblyX.Entry;
            var title = asmx?.Asm.GetCustomAttribute<AssemblyTitleAttribute>();
            var dis = asmx?.Asm.GetCustomAttribute<DisplayNameAttribute>();
            var des = asmx?.Asm.GetCustomAttribute<DescriptionAttribute>();
            var dname = title?.Title ?? dis?.DisplayName ?? des?.Description;

            var arg = new LoginModel
            {
                User = UserName,
                Pass = Password.IsNullOrEmpty() ? null : Password.MD5(),
                DisplayName = dname,
                Machine = Environment.MachineName,
                ProcessId = Process.GetCurrentProcess().Id,
                Version = asmx.Version,
                Compile = asmx.Compile,
            };

            var rs = await base.InvokeWithClientAsync<LoginResponse>(client, "Login", arg);

            var set = AntSetting.Current;
            if (set.Debug) XTrace.WriteLine("登录{0}成功！{1}", client, rs.ToJson());

            // 保存下发密钥
            if (!rs.Secret.IsNullOrEmpty())
            {
                set.Secret = rs.Secret;
                set.Save();
            }

            Logined = true;

            return Info = rs;
        }
        #endregion

        #region 核心方法
        /// <summary>获取指定名称的作业</summary>
        /// <returns></returns>
        public IJob[] GetJobs() => Invoke<JobModel[]>(nameof(GetJobs));

        /// <summary>批量添加作业</summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public String[] AddJobs(IJob[] jobs) => Invoke<String[]>(nameof(AddJobs), new { jobs });

        /// <summary>申请作业任务</summary>
        /// <param name="job">作业</param>
        /// <param name="count">要申请的任务个数</param>
        /// <param name="ext">扩展数据</param>
        /// <returns></returns>
        public ITask[] Acquire(String job, Int32 count, Object ext = null)
        {
            var dic = new { job, count }.ToDictionary();
            if (ext != null) dic = dic.Merge(ext);

            return Invoke<TaskModel[]>(nameof(Acquire), dic);
        }

        /// <summary>生产消息</summary>
        /// <param name="job">作业</param>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        public Int32 Produce(String job, String topic, String[] messages, MessageOption option = null)
        {
            var dic = new { job, topic, messages }.ToDictionary();
            if (option != null) dic = dic.Merge(option);

            return Invoke<Int32>(nameof(Produce), dic);
        }

        /// <summary>报告状态（进度、成功、错误）</summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Boolean Report(ITaskResult task)
        {
            var retry = 3;
            var lastex = new Exception();
            while (retry-- > 0)
            {
                try
                {
                    return Invoke<Boolean>(nameof(Report), task);
                }
                catch (Exception ex)
                {
                    lastex = ex;
                }
            }

            throw lastex;
        }

        /// <summary>获取当前应用的所有在线实例</summary>
        /// <returns></returns>
        public IPeer[] GetPeers() => Invoke<PeerModel[]>(nameof(GetPeers));
        #endregion
    }
}