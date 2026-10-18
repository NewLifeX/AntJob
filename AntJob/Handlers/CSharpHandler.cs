using AntJob.Data;
using NewLife;
using NewLife.Caching;
#if !NET45
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
#endif

namespace AntJob.Handlers;

/// <summary>C#代码处理器，定时执行一段C#代码</summary>
/// <remarks>
/// 应用型处理器，可直接使用。
/// 脚本代码取自任务数据（<see cref="JobContext.Data"/> 或 <see cref="ITask.Data"/>），脚本内可通过 <c>ctx</c> 访问当前作业上下文。
/// 脚本返回值作为本次成功处理数；无返回值或非数值返回值按 1 计。
/// 注意：脚本以当前进程身份直接执行，不做安全沙箱隔离，仅可用于可信代码。
/// </remarks>
public class CSharpHandler : Handler
{
    #region 属性
    private static readonly ICache _cache = MemoryCache.Default;
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
    /// <returns>成功处理数。无脚本代码时返回-1</returns>
    /// <exception cref="NotSupportedException">net45 不支持脚本执行</exception>
    public override Int32 Execute(JobContext ctx)
    {
        if (ctx == null) return -1;

        var code = (ctx.Data ?? ctx.Task?.Data) as String;
        if (code.IsNullOrWhiteSpace()) return -1;

#if NET45
        throw new NotSupportedException("net45 不支持C#脚本执行，请使用 net461 及以上版本");
#else
        var script = _cache.GetOrAdd($"CSharpHandler:{code}", CreateScript, 24 * 3600);
        var state = script.RunAsync(new ScriptGlobals { ctx = ctx, Context = ctx }).ConfigureAwait(false).GetAwaiter().GetResult();

        var value = state.ReturnValue;
        if (value == null) return 1;

        // 数值返回值作为成功处理数，其它类型按1计
        if (value is Int32 or Int64 or Int16 or Byte or Double or Single or Decimal) return value.ToInt();

        return 1;
#endif
    }

#if !NET45
    // 注意：GetOrAdd 传给工厂方法的是缓存键（含前缀），需要还原脚本代码
    private static Script<Object> CreateScript(String key) => CSharpScript.Create<Object>(key.TrimPrefix("CSharpHandler:"), ScriptOptions.Default
        .AddReferences(typeof(Object).Assembly, typeof(Enumerable).Assembly, typeof(JobContext).Assembly)
        .AddImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "AntJob", "AntJob.Data"), typeof(ScriptGlobals));

    /// <summary>脚本全局变量。脚本内可通过 ctx 或 Context 访问作业上下文</summary>
    public class ScriptGlobals
    {
        /// <summary>作业上下文</summary>
        public JobContext ctx { get; set; }

        /// <summary>作业上下文</summary>
        public JobContext Context { get; set; }
    }
#endif
}
