using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NewLife.Log;
using NewLife.Net;
using NewLife.Reflection;
using NewLife.Remoting;
using NewLife.Serialization;

namespace AntJob
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
        public IDictionary<String, Object> Info { get; private set; }
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

            var arg = new
            {
                user = UserName,
                pass = Password.MD5(),
                machine = Environment.MachineName,
                processid = Process.GetCurrentProcess().Id,
                version = asmx?.Version,
            };

            var rs = await base.InvokeWithClientAsync<Object>(client, "Login", arg);
            if (Setting.Current.Debug) XTrace.WriteLine("登录{0}成功！{1}", client, rs.ToJson());

            Logined = true;

            return Info = rs as IDictionary<String, Object>;
        }
        #endregion

        #region 核心方法
        /// <summary>获取指定名称的作业</summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public IJob[] GetJobs(String[] names)
        {
            if (names == null) names = new String[0];

            return Invoke<MyJob[]>(nameof(GetJobs), new { names });
        }

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

            return Invoke<MyTask[]>(nameof(Acquire), dic);
        }

        /// <summary>生产消息</summary>
        /// <param name="topic">主题</param>
        /// <param name="messages">消息集合</param>
        /// <param name="option">消息选项</param>
        /// <returns></returns>
        public Int32 Produce(String topic, String[] messages, MessageOption option = null)
        {
            var dic = new { topic, messages }.ToDictionary();
            if (option != null) dic = dic.Merge(option);

            return Invoke<Int32>(nameof(Produce), dic);
        }

        /// <summary>报告状态（进度、成功、错误）</summary>
        /// <param name="task"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public Boolean Report(ITask task, Object ext = null)
        {
            var dic = new { item = task }.ToDictionary();
            if (ext != null) dic = dic.Merge(ext);

            var retry = 3;
            var lastex = new Exception();
            while (retry-- > 0)
            {
                try
                {
                    return Invoke<Boolean>(nameof(Report), dic);
                }
                catch (Exception ex)
                {
                    lastex = ex;
                }
            }

            throw lastex;
        }
        #endregion
    }
}