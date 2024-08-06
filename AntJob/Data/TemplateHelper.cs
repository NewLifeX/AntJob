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
            var ti = Find(str, "DataTime", p);
            if (ti.IsEmpty) ti = Find(str, "dt", p);
            if (ti.IsEmpty)
            {
                sb.Append(str.Substring(p));
                break;
            }

            // 准备替换
            var val = ti.Format.IsNullOrEmpty() ? startTime.ToFullString() : startTime.ToString(ti.Format);
            sb.Append(str.Substring(p, ti.Start - p));
            sb.Append(val);

            // 移动指针
            p = ti.End + 1;
        }

        str = sb.ToString();
        sb.Clear();
        p = 0;
        while (true)
        {
            var ti = Find(str, "End", p);
            if (ti.IsEmpty)
            {
                sb.Append(str.Substring(p));
                break;
            }

            // 准备替换
            var val = ti.Format.IsNullOrEmpty() ? endTime.ToFullString() : endTime.ToString(ti.Format);
            sb.Append(str.Substring(p, ti.Start - p));
            sb.Append(val);

            // 移动指针
            p = ti.End + 1;
        }

        return sb.Put(true);
    }

    private static VarItem Find(String str, String key, Int32 p)
    {
        // 头尾
        var p1 = str.IndexOf("{" + key, p);
        if (p1 < 0) return _empty;

        var p2 = str.IndexOf("}", p1);
        if (p2 < 0) return _empty;

        // 格式化字符串
        var format = "";
        var p3 = str.IndexOf(":", p1);
        if (p3 > 0 && p3 < p2) format = str.Substring(p3 + 1, p2 - p3 - 1);

        // 左括号位置，右括号位置，格式化字符串
        return new VarItem(p1, p2, format);
    }

    private static VarItem _empty = new(-1, -1, "");
    struct VarItem(Int32 start, Int32 end, String format)
    {
        public Int32 Start = start;
        public Int32 End = end;
        public String Format = format;

        public readonly Boolean IsEmpty => Start < 0;
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