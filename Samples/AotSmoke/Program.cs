using System.Globalization;
using AntJob;
using AntJob.Data;
using AntJob.Extensions;
using NewLife;

namespace AotSmoke;

/// <summary>NativeAOT 冒烟：验证主库、扩展库与数据模型在裁剪后仍可加载、翻译与调度</summary>
public static class Program
{
    /// <summary>执行冒烟</summary>
    /// <returns>失败数，0 表示通过</returns>
    public static Int32 Main()
    {
        var fail = 0;

        void Check(String name, Action action)
        {
            try
            {
                action();
                Console.WriteLine($"[PASS] {name}");
            }
            catch (Exception ex)
            {
                fail++;
                Console.WriteLine($"[FAIL] {name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        Console.WriteLine("== AntJob NativeAOT 冒烟 ==");
        Console.WriteLine($"Runtime = {Environment.Version}, Arch = {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

        // 编译期词表在裁剪后仍能命中译文，英文源文保持不变
        Check("localization", () =>
        {
            const String source = "No handler is available.";

            Localization.Culture = new CultureInfo("zh-Hans");
            var zh = Localization.Translate(source);
            if (zh == source) throw new Exception("zh-Hans translation table was trimmed");
            Console.WriteLine($"  zh-Hans => {zh}");

            Localization.Culture = new CultureInfo("en-US");
            if (Localization.Translate(source) != source) throw new Exception("English source text was altered");
        });

        // 调度器构造，并按类型注册处理器（依赖注入路径）
        Check("scheduler", () =>
        {
            using var sc = new Scheduler();
            sc.AddHandler<AotHandler>();
            if (sc.Handlers == null) throw new Exception("handler collection was trimmed");
        });

        // 扩展库处理器可实例化，作业元数据未被裁剪
        Check("extensions", () =>
        {
            Handler[] handlers = [new AotHandler(), new SqlHandler(), new SqlMessage()];
            foreach (var handler in handlers)
            {
                var job = handler.Job;
                if (job == null) throw new Exception($"job metadata of {handler.GetType().Name} was trimmed");
            }

            // 扩展库自带词表，同样需要在裁剪后可用
            Localization.Culture = new CultureInfo("zh-Hans");
            var text = Localization.Translate("Data extraction is not configured.");
            if (text == "Data extraction is not configured.") throw new Exception("extensions translation table was trimmed");
            Console.WriteLine($"  zh-Hans => {text}");
        });

        // 数据模型的时间表达式模板
        Check("time-expression", () =>
        {
            var exp = new TimeExpression("{dt:yyyyMMdd}");
            var text = exp.Build(new DateTime(2026, 10, 28));
            if (text != "20261028") throw new Exception($"unexpected template result: {text}");
        });

        Console.WriteLine(fail == 0 ? "== 全部通过 ==" : $"== 失败 {fail} 项 ==");

        return fail;
    }
}

/// <summary>冒烟用处理器</summary>
public class AotHandler : Handler
{
    /// <inheritdoc/>
    public override Int32 Execute(JobContext ctx) => 0;
}
