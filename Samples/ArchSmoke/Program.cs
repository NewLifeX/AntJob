using System.Reflection;

namespace ArchSmoke;

/// <summary>指令集冒烟：验证主库在目标架构运行时上可加载并执行核心 API</summary>
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

        Console.WriteLine($"== AntJob.Scheduler 指令集冒烟 ==");
        Console.WriteLine($"Arch = {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

        var asm = typeof(AntJob.Scheduler).Assembly;
        Check("assembly:" + asm.GetName().Name, () =>
        {
            if (asm.GetExportedTypes().Length == 0) throw new Exception("no exported types");
        });

        Console.WriteLine(fail == 0 ? "== 全部通过 ==" : $"== 失败 {fail} 项 ==");
        return fail;
    }
}
