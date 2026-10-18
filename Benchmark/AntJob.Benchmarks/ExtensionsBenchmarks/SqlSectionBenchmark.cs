using AntJob.Extensions;
using BenchmarkDotNet.Attributes;

namespace Benchmark.ExtensionsBenchmarks;

/// <summary>SQL 片段解析基准。SqlHandler/SqlMessage 每次执行都会解析 SQL 文本</summary>
[MemoryDiagnoser]
[SimpleJob]
public class SqlSectionBenchmark
{
    private String _single;
    private String _multi;

    [GlobalSetup]
    public void Setup()
    {
        _single = "/*use membership*/" + Environment.NewLine + "select * from role where update_time>='{dt}' and update_time<'{End}'";
        _multi = """
            /*use membership*/
            select * from role where update_time>='{dt}' and update_time<'{End}'

            /*use membership*/
            delete from role_bak where update_time>='{dt}'

            /*use membership*/
            insert role_bak

            /*use membership*/
            select id as topic_roleId, name from role where update_time>='{dt}'
            """;
    }

    [Benchmark(Description = "解析单条 SQL")]
    public Int32 ParseSingle() => SqlSection.ParseAll(_single).Length;

    [Benchmark(Description = "解析四条片段 SQL")]
    public Int32 ParseMulti() => SqlSection.ParseAll(_multi).Length;
}
