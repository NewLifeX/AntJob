using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AntJob.Data;
using AntJob.Models;
using NewLife;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Remoting.Clients;
using NewLife.Remoting.Models;

namespace AntJob.Providers;

/// <summary>蚂蚁客户端</summary>
public class AntClient : ClientBase
{
    private readonly AntSetting _setting;
    #region 属性
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public AntClient() => Prefix = "Ant/";

    /// <summary>实例化</summary>
    /// <param name="setting"></param>
    public AntClient(AntSetting setting) : base(setting)
    {
        _setting = setting;
        Prefix = _setting.Server.StartsWithIgnoreCase("http://", "https://") ? "AntJob/" : "Ant/";
    }
    #endregion

    #region 方法
    /// <summary>初始化</summary>
    protected override void OnInit()
    {
        var provider = ServiceProvider ??= ObjectContainer.Provider;

        // 找到容器，注册默认的模型实现，供后续InvokeAsync时自动创建正确的模型对象
        var container = ModelExtension.GetService<IObjectContainer>(provider) ?? ObjectContainer.Current;
        if (container != null)
        {
            container.TryAddTransient<ILoginRequest, LoginModel>();
            //container.TryAddTransient<ILoginResponse, LoginResponse>();
            //container.TryAddTransient<ILogoutResponse, LogoutResponse>();
            //container.TryAddTransient<IPingRequest, PingInfo>();
            //container.TryAddTransient<IPingResponse, PingResponse>();
            //container.TryAddTransient<IUpgradeInfo, UpgradeInfo>();
        }

        Prefix = _setting.Server.StartsWithIgnoreCase("http://", "https://") ? "AntJob/" : "Ant/";

        base.OnInit();
    }
    #endregion

    #region 登录
    /// <summary>创建登录请求</summary>
    /// <returns></returns>
    public override ILoginRequest BuildLoginRequest()
    {
        var request = base.BuildLoginRequest();
        if (request is LoginModel model)
        {
            var asmx = AssemblyX.Entry;
            var title = asmx?.Asm.GetCustomAttribute<AssemblyTitleAttribute>();
            var dis = asmx?.Asm.GetCustomAttribute<DisplayNameAttribute>();
            var des = asmx?.Asm.GetCustomAttribute<DescriptionAttribute>();
            var dname = title?.Title ?? dis?.DisplayName ?? des?.Description;

            model.DisplayName = dname;
            model.Machine = Environment.MachineName;
            model.ProcessId = Process.GetCurrentProcess().Id;
            model.Compile = asmx.Compile;
        }

        return request;
    }
    #endregion

    #region 核心方法
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    public IJob[] GetJobs() => InvokeAsync<JobModel[]>(nameof(GetJobs)).Result;

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    public String[] AddJobs(IJob[] jobs) => InvokeAsync<String[]>(nameof(AddJobs), new { jobs }).Result;

    /// <summary>申请作业任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public ITask[] Acquire(String job, String topic, Int32 count) => InvokeAsync<TaskModel[]>(nameof(Acquire), new AcquireModel
    {
        Job = job,
        Topic = topic,
        Count = count,
    }).Result;

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public Int32 Produce(ProduceModel model) => InvokeAsync<Int32>(nameof(Produce), model).Result;

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
                return InvokeAsync<Boolean>(nameof(Report), task).Result;
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
    public IPeer[] GetPeers() => InvokeAsync<PeerModel[]>(nameof(GetPeers)).Result;
    #endregion
}