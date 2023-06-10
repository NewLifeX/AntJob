using NewLife;
using NewLife.Collections;

namespace AntJob.Data;

/// <summary>模板助手</summary>
public static class TemplateHelper
{
    /// <summary>使用时间参数处理模板</summary>
    /// <param name="template"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public static String Build(String template, DateTime startTime, DateTime endTime)
    {
        if (template.IsNullOrEmpty()) return template;

        var str = template;
        var sb = Pool.StringBuilder.Get();
        var p = 0;
        while (true)
        {
            var ti = Find(str, "Start", p);
            if (ti == null)
            {
                sb.Append(str.Substring(p));
                break;
            }

            // 准备替换
            var val = ti.Item3.IsNullOrEmpty() ? startTime.ToFullString() : startTime.ToString(ti.Item3);
            sb.Append(str.Substring(p, ti.Item1 - p));
            sb.Append(val);

            // 移动指针
            p = ti.Item2 + 1;
        }

        str = sb.ToString();
        sb.Clear();
        p = 0;
        while (true)
        {
            var ti = Find(str, "End", p);
            if (ti == null)
            {
                sb.Append(str.Substring(p));
                break;
            }

            // 准备替换
            var val = ti.Item3.IsNullOrEmpty() ? endTime.ToFullString() : endTime.ToString(ti.Item3);
            sb.Append(str.Substring(p, ti.Item1 - p));
            sb.Append(val);

            // 移动指针
            p = ti.Item2 + 1;
        }

        return sb.Put(true);
    }

    private static Tuple<Int32, Int32, String> Find(String str, String key, Int32 p)
    {
        // 头尾
        var p1 = str.IndexOf("{" + key, p);
        if (p1 < 0) return null;

        var p2 = str.IndexOf("}", p1);
        if (p2 < 0) return null;

        // 格式化字符串
        var format = "";
        var p3 = str.IndexOf(":", p1);
        if (p3 > 0 && p3 < p2) format = str.Substring(p3 + 1, p2 - p3 - 1);

        // 左括号位置，右括号位置，格式化字符串
        return new Tuple<Int32, Int32, String>(p1, p2, format);
    }

    /// <summary>使用消息数组处理模板</summary>
    /// <param name="template"></param>
    /// <param name="messages"></param>
    /// <returns></returns>
    public static String Build(String template, String[] messages)
    {
        if (template.IsNullOrEmpty()) return template;

        var str = template;
        var sb = Pool.StringBuilder.Get();
        var p = 0;
        while (true)
        {
            var p1 = str.IndexOf("{Message}", p);
            if (p1 < 0)
            {
                sb.Append(str.Substring(p));
                break;
            }

            // 准备替换
            var val = messages.Join();
            sb.Append(str.Substring(p, p1 - p));
            sb.Append(val);

            // 移动指针
            p = p1 + "{Message}".Length;
        }

        return sb.Put(true);
    }
}