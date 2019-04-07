using System;
using System.Collections.Generic;
using NewLife.Collections;
using NewLife.Data;

namespace AntJob
{
    /// <summary>作业上下文</summary>
    public class JobContext : IExtend
    {
        #region 属性
        /// <summary>作业</summary>
        public Job Job { get; set; }

        /// <summary>抽取设置</summary>
        public ITask Setting { get; set; }

        /// <summary>状态</summary>
        public JobStatus Status { get; set; }

        /// <summary>列表数据</summary>
        public Object Data { get; set; }

        /// <summary>处理总数</summary>
        public Int32 Total { get; set; }

        /// <summary>成功处理数</summary>
        public Int32 Success { get; set; }

        /// <summary>总耗时，毫秒</summary>
        public Double TotalCost { get; set; }

        /// <summary>抽取耗时，毫秒</summary>
        public Double FetchCost { get; set; }

        /// <summary>处理耗时</summary>
        public Double ProcessCost { get; set; }

        /// <summary>最后处理键值。由业务决定，便于分析问题</summary>
        public String Key { get; set; }

        /// <summary>当前处理对象</summary>
        public Object Entity { get; set; }

        /// <summary>处理异常</summary>
        public Exception Error { get; set; }

        /// <summary>任务备注消息。可用于保存到任务项内容字段</summary>
        public String Remark { get; set; }
        #endregion

        #region 索引器
        /// <summary>用户数据</summary>
        public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>用户数据</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Object this[String item] { get => Items[item]; set => Items[item] = value; }
        #endregion

        #region 扩展属性
        /// <summary>抽取速度</summary>
        public Int32 FetchSpeed => (FetchCost <= 0 || Total == 0) ? 0 : (Int32)Math.Min(Total * 1000L / FetchCost, Int32.MaxValue);

        /// <summary>处理速度</summary>
        public Int32 ProcessSpeed => (ProcessCost <= 0 || Total == 0) ? 0 : (Int32)Math.Min(Total * 1000L / ProcessCost, Int32.MaxValue);
        #endregion
    }
}