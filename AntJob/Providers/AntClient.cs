using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AntJob.Data;
using AntJob.Models;
using NewLife;
using NewLife.Data;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Remoting.Clients;
using NewLife.Remoting.Models;

namespace AntJob.Providers;

/// <summary>蚂蚁客户端</summary>
public class AntClient : ClientBase
{
    #region 属性
    private readonly AntSetting _setting;

    /// <summary>是否使用 HTTP 通道。地址以 http/https 开头时启用</summary>
    public Boolean IsHttp => Server.StartsWithIgnoreCase("http://", "https://");

    private volatile Boolean _notifyConnected;
    /// <summary>下行通知通道是否已连接</summary>
    public Boolean NotifyConnected => _notifyConnected;

    /// <summary>下行通知通道连接成功事件。首次连接或断线重连后触发</summary>
    public event EventHandler NotifyConnectedChanged;
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public AntClient() => InitClient();

    /// <summary>实例化</summary>
    /// <param name="setting"></param>
    public AntClient(AntSetting setting) : base(setting)
    {
        _setting = setting;

        InitClient();
    }

    private void InitClient()
    {
        Features = Features.Login | Features.Logout | Features.Ping;

        if (IsHttp)
        {
            // HTTP 通道通过 WebSocket 接收下行通知。携带客户端标识，便于服务端把同一实例的重连映射到同一会话
            Features |= Features.Notify;
            SetActions("AntJob/");
            Actions[Features.Notify] = "AntJob/Notify?clientId=" + Uri.EscapeDataString(Runtime.ClientId);
        }
        else
            SetActions("");
    }

    /// <summary>获取业务接口路径。HTTP 通道统一加 AntJob 前缀</summary>
    /// <param name="name">接口名</param>
    /// <returns></returns>
    private String GetAction(String name) => IsHttp ? "AntJob/" + name : name;
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

        InitClient();

        base.OnInit();
    }

    /// <summary>心跳。维持在线状态，并维护 HTTP 通道的 WebSocket 长连接</summary>
    /// <remarks>
    /// HTTP 通道不走基类的 WebSocket/SSE 自动降级，避免在网关不支持 WebSocket 时反复请求 SSE 端点。
    /// 这里通过 SendAsync 间接维护 WebSocket，并发送 Ping 文本消息供服务端刷新会话。
    /// </remarks>
    /// <param name="state"></param>
    /// <returns></returns>
    protected override async Task OnPing(Object state)
    {
        if (Features.HasFlag(Features.Ping)) await Ping().ConfigureAwait(false);

        if (!IsHttp) return;

        try
        {
            await SendAsync(new ArrayPacket("Ping".GetBytes())).ConfigureAwait(false);

            // 首次连接或断线重连成功后，通知提供者立即拉取一次，弥补断线期间可能丢失的通知
            if (!_notifyConnected)
            {
                _notifyConnected = true;
                NotifyConnectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _notifyConnected = false;
            Log?.Debug("[{0}]WebSocket心跳失败：{1}", Name, ex.Message);
        }
    }

    /// <summary>心跳。HTTP 通道通过查询参数携带客户端标识，服务端据此区分同IP多实例</summary>
    /// <param name="request">心跳请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    protected override Task<IPingResponse> PingAsync(IPingRequest request, CancellationToken cancellationToken)
    {
        if (!IsHttp) return base.PingAsync(request, cancellationToken);

        return InvokeAsync<IPingResponse>(Actions[Features.Ping] + "?clientId=" + Uri.EscapeDataString(Runtime.ClientId), request, cancellationToken);
    }

    /// <summary>注销。HTTP 通道同样携带客户端标识，及时清理对应在线记录</summary>
    /// <param name="reason">原因</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    protected override Task<ILogoutResponse> LogoutAsync(String reason, CancellationToken cancellationToken)
    {
        if (!IsHttp) return base.LogoutAsync(reason, cancellationToken);

        var args = new Dictionary<String, Object>
        {
            ["reason"] = reason,
            ["clientId"] = Runtime.ClientId,
        };

        return GetAsync<ILogoutResponse>(Actions[Features.Logout], args, cancellationToken);
    }
    #endregion

    #region 登录
    /// <summary>创建登录请求</summary>
    /// <returns></returns>
    public override ILoginRequest BuildLoginRequest()
    {
        var request = new LoginModel();
        FillLoginRequest(request);

        var asmx = AssemblyX.Entry;
        var title = asmx?.Asm.GetCustomAttribute<AssemblyTitleAttribute>();
        var dis = asmx?.Asm.GetCustomAttribute<DisplayNameAttribute>();
        var des = asmx?.Asm.GetCustomAttribute<DescriptionAttribute>();
        var dname = title?.Title ?? dis?.DisplayName ?? des?.Description;

        request.DisplayName = dname;
        request.Machine = Environment.MachineName;
        request.ProcessId = Process.GetCurrentProcess().Id;
        //request.Compile = asmx.Compile;

        return request;
    }
    #endregion

    #region 核心方法
    /// <summary>获取指定名称的作业</summary>
    /// <returns></returns>
    public async Task<IJob[]> GetJobs() => await InvokeAsync<JobModel[]>(GetAction(nameof(GetJobs)));

    /// <summary>批量添加作业</summary>
    /// <param name="jobs"></param>
    /// <returns></returns>
    public Task<String[]> AddJobs(IJob[] jobs) => IsHttp
        // HTTP 接口直接接收数组，RPC 接口使用参数对象
        ? InvokeAsync<String[]>(GetAction(nameof(AddJobs)), jobs)
        : InvokeAsync<String[]>(GetAction(nameof(AddJobs)), new { jobs });

    /// <summary>设置作业。支持控制作业启停、数据时间、步进等参数</summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public async Task<IJob> SetJob(IDictionary<String, Object> job) => await InvokeAsync<JobModel>(GetAction(nameof(SetJob)), job);

    /// <summary>申请作业任务</summary>
    /// <param name="job">作业</param>
    /// <param name="topic">主题</param>
    /// <param name="count">要申请的任务个数</param>
    /// <returns></returns>
    public async Task<ITask[]> Acquire(String job, String topic, Int32 count)
    {
        // HTTP 通道携带客户端标识，服务端据此绑定登录时创建的在线实例
        var action = GetAction(nameof(Acquire));
        if (IsHttp) action += "?clientId=" + Uri.EscapeDataString(Runtime.ClientId);

        return await InvokeAsync<TaskModel[]>(action, new AcquireModel
        {
            Job = job,
            Topic = topic,
            Count = count,
        });
    }

    /// <summary>生产消息</summary>
    /// <param name="model">模型</param>
    /// <returns></returns>
    public Task<Int32> Produce(ProduceModel model) => InvokeAsync<Int32>(GetAction(nameof(Produce)), model);

    /// <summary>报告状态（进度、成功、错误）</summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public async Task<Boolean> Report(ITaskResult task)
    {
        // HTTP 通道携带客户端标识，统计才能累计到登录时创建的在线实例
        var action = GetAction(nameof(Report));
        if (IsHttp) action += "?clientId=" + Uri.EscapeDataString(Runtime.ClientId);

        var retry = 3;
        var lastex = new Exception();
        while (retry-- > 0)
        {
            try
            {
                return await InvokeAsync<Boolean>(action, task);
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
    public async Task<IPeer[]> GetPeers() => await InvokeAsync<PeerModel[]>(GetAction(nameof(GetPeers)));
    #endregion
}