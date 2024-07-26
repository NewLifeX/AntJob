using NewLife.Remoting.Models;

namespace AntJob.Models;

/// <summary>登录模型</summary>
public class LoginModel : LoginRequest
{
    /// <summary>用户名</summary>
    public String User { get; set; }

    /// <summary>用户名</summary>
    public String Pass { get; set; }

    /// <summary>显示名</summary>
    public String DisplayName { get; set; }

    /// <summary>机器名</summary>
    public String Machine { get; set; }

    /// <summary>进程Id</summary>
    public Int32 ProcessId { get; set; }

    ///// <summary>版本</summary>
    //public String Version { get; set; }

    ///// <summary>编译时间</summary>
    //public DateTime Compile { get; set; }
}

///// <summary>登录响应</summary>
//public class LoginResponse
//{
//    /// <summary>名称</summary>
//    public String Name { get; set; }

//    /// <summary>密钥。仅注册时返回</summary>
//    public String Secret { get; set; }

//    /// <summary>显示名</summary>
//    public String DisplayName { get; set; }
//}