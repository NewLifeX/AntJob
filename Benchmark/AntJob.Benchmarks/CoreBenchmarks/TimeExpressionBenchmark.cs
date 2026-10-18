using AntJob.Data;
using BenchmarkDotNet.Attributes;

namespace Benchmark.CoreBenchmarks;

/// <summary>时间表达式与模板替换基准。它们是每次切片/每次 SQL 执行都会走的热点</summary>
[MemoryDiagnoser]
[SimpleJob]
public class TimeExpressionBenchmark
{
    private DateTime _time;
    private TimeExpression _expression;
    private TimeExpression _expressionFull;

    [GlobalSetup]
    public void Setup()
    {
        _time = new DateTime(2026, 10, 18, 11, 30, 0);
        _expression = new TimeExpression("{dt+1M+5d:yyyyMMdd}");
        _expressionFull = new TimeExpression("{dt+1M+5d}");
    }

    [Benchmark(Description = "解析并执行简单时间表达式")]
    public DateTime ParseAndExecute()
    {
        var exp = new TimeExpression("{dt+1M+5d:yyyyMMdd}");
        return exp.Execute(_time);
    }

    [Benchmark(Description = "执行已解析表达式（复用解析结果）")]
    public DateTime ExecuteParsed() => _expression.Execute(_time);

    [Benchmark(Description = "执行已解析表达式并格式化输出")]
    public String ExecuteAndFormat() => _expressionFull.Build(_time);

    [Benchmark(Description = "替换模板时间变量 {dt}/{End}")]
    public String BuildTimeTemplate() => TemplateHelper.Build("select * from role where update_time>='{dt:yyyy-MM-dd HH:mm:ss}' and update_time<'{End:yyyy-MM-dd HH:mm:ss}'", _time, _time.AddMinutes(30));

    [Benchmark(Description = "对照：单次日期格式化")]
    public String FormatDate() => _time.ToString("yyyy-MM-dd HH:mm:ss");

    [Benchmark(Description = "对照：字符串查找与截取")]
    public Int32 StringIndexOf()
    {
        var str = "select * from role where update_time>='{dt:yyyy-MM-dd HH:mm:ss}' and update_time<'{End:yyyy-MM-dd HH:mm:ss}'";
        var p1 = str.IndexOf("{dt");
        var p2 = str.IndexOf('}', p1);
        return p2 - p1 + str.Substring(p1, p2 - p1).Length;
    }

    [Benchmark(Description = "对照：Ordinal 字符串查找")]
    public Int32 StringIndexOfOrdinal()
    {
        var str = "select * from role where update_time>='{dt:yyyy-MM-dd HH:mm:ss}' and update_time<'{End:yyyy-MM-dd HH:mm:ss}'";
        var p1 = str.IndexOf("{dt", StringComparison.Ordinal);
        var p2 = str.IndexOf('}', p1);
        return p2 - p1 + str.Substring(p1, p2 - p1).Length;
    }

    [Benchmark(Description = "替换模板消息变量 {Message}")]
    public String BuildMessageTemplate() => TemplateHelper.Build("update role set name='{Message}' where id={Message}", ["1", "2", "3"]);
}
