using AntJob.Data;
using NewLife;

namespace AntJob.Handlers;

/// <summary>C#代码处理器，定时执行一段C#代码</summary>
/// <remarks>
/// 应用型处理器，可直接使用。
/// 注意：当前版本仅保留骨架，主体执行逻辑因安全沙箱设计未定而暂缓实现。
/// 详见需求文档「暂缓清单」。
/// </remarks>
public class CSharpHandler : Handler
{
    #region 属性
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public CSharpHandler()
    {
        Mode = JobModes.Time;

        var job = Job;
        job.BatchSize = 8;
    }
    #endregion

    /// <summary>执行</summary>
    /// <param name="ctx">作业上下文</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">C#脚本执行功能暂缓实现，安全沙箱需设计</exception>
    public override Int32 Execute(JobContext ctx)
    {
        var code = ctx.Data as String;
        if (code.IsNullOrWhiteSpace()) return -1;

        // C#脚本执行功能暂缓实现
        // 需要设计安全沙箱隔离策略（AppDomain/AssemblyLoadContext + 权限限制）
        // 参考需求文档「暂缓清单」
        throw new NotImplementedException("C#脚本执行功能暂缓实现，需要先设计安全沙箱隔离策略。");
    }
}