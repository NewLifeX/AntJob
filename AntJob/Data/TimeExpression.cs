using NewLife;

namespace AntJob.Data;

/// <summary>时间表达式，一次解析多次使用。如{dt+1M+5d:yyyyMMdd}</summary>
public class TimeExpression
{
    #region 属性
    /// <summary>表达式</summary>
    public String Expression { get; set; }

    /// <summary>变量名</summary>
    public String VarName { get; set; } = "dt";

    /// <summary>格式化字符串</summary>
    public String Format { get; set; }

    /// <summary>时间表达式项集合</summary>
    public IList<TimeExpressionItem> Items { get; } = [];
    #endregion

    #region 构造
    /// <summary>实例化时间表达式</summary>
    public TimeExpression() { }

    /// <summary>实例化时间表达式</summary>
    public TimeExpression(String expression) => Parse(expression);
    #endregion

    #region 方法
    /// <summary>解析表达式</summary>
    public Boolean Parse(String expression)
    {
        var p1 = expression.IndexOf('{');
        if (p1 < 0) return false;

        var p2 = expression.IndexOf('}', p1);
        if (p2 < 0) return false;

        expression = expression.Substring(p1 + 1, p2 - p1 - 1);

        // 循环查找
        var ms = Items;
        p1 = -1;
        while (true)
        {
            p2 = expression.IndexOfAny(['+', '-', ':', ','], p1 + 1);
            if (p2 < 0) p2 = expression.Length;

            // 第一段是变量名
            if (p1 < 0 && p2 > 0)
            {
                VarName = expression[0..p2];
            }
            else if (expression[p1] is '+' or '-')
            {
                var str = expression[p1..p2];
                var item = new TimeExpressionItem();
                if (!item.Parse(str)) throw new InvalidDataException($"Invalid [{str}]");

                ms.Add(item);
            }
            else if (expression[p1] is ':' or ',')
            {
                // 最后一段是格式化字符串
                p2 = expression.Length;
                Format = expression[(p1 + 1)..p2];
            }

            if (p2 >= expression.Length) break;
            p1 = p2;
        }

        // 默认天级
        if (ms.Count == 0) ms.Add(new TimeExpressionItem { Level = "d", Value = 0 });

        Expression = expression;

        return true;
    }

    /// <summary>执行时间偏移</summary>
    public DateTime Execute(DateTime time)
    {
        foreach (var item in Items)
        {
            time = item.Execute(time);
        }

        return time;
    }

    /// <summary>构建时间字符串</summary>
    public String Build(DateTime time)
    {
        time = Execute(time);

        var ms = Items;
        var format = Format;
        if (format.IsNullOrEmpty() && ms.Count > 0) format = ms[ms.Count - 1].GetFormat();
        if (format.IsNullOrEmpty()) format = "yyyyMMdd";

        return time.ToString(format);
    }
    #endregion

    /// <summary>处理时间偏移模版。如{dt+1M+5d:yyyyMMdd}</summary>
    /// <param name="template"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static String Build(String template, DateTime time)
    {
        return null;
    }
}

/// <summary>时间表达式项。如+5d</summary>
public class TimeExpressionItem
{
    /// <summary>级别。如y/M/d/H/m/w</summary>
    public String Level { get; set; }

    /// <summary>数值。包括正负</summary>
    public Int32 Value { get; set; }

    /// <summary>分解表达式项。如+5d</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Boolean Parse(String value)
    {
        if (value.IsNullOrEmpty() || value.Length < 3) return false;

        Level = value[^1..];
        Value = value[..^1].TrimStart('+').ToInt();

        return true;
    }

    /// <summary>执行时间偏移</summary>
    public DateTime Execute(DateTime time)
    {
        return Level switch
        {
            "y" => new DateTime(time.Year, 1, 1, 0, 0, 0, time.Kind).AddYears(Value),
            "M" => new DateTime(time.Year, time.Month, 1, 0, 0, 0, time.Kind).AddMonths(Value),
            "d" => new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind).AddDays(Value),
            "H" => new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, time.Kind).AddHours(Value),
            "m" => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind).AddMinutes(Value),
            "s" => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind).AddSeconds(Value),
            "w" => new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind).AddDays(Value * 7),
            _ => time,
        };
    }

    /// <summary>获取格式化字符串</summary>
    public String GetFormat()
    {
        return Level switch
        {
            "y" => "yyyy",
            "M" => "yyyyMM",
            "d" => "yyyyMMdd",
            "H" => "yyyyMMddHH",
            "m" => "yyyyMMddHHmm",
            "s" => "yyyyMMddHHmmss",
            "w" => "yyyyww",
            _ => "",
        };
    }
}