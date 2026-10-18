using BenchmarkDotNet.Running;

namespace Benchmark;

public class Program
{
    public static void Main(String[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
