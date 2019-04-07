using System;
using System.Collections;
using XCode;
using XCode.Configuration;

namespace AntJob
{
    /// <summary>从数据库抽取数据</summary>
    public abstract class DataJob : Job
    {
        #region 属性
        /// <summary>实体工厂</summary>
        public IEntityOperate Factory { get; set; }

        /// <summary>附加条件</summary>
        public String Where { get; set; }

        /// <summary>时间字段</summary>
        public FieldItem Field { get; set; }

        /// <summary>排序</summary>
        public String OrderBy { get; set; }

        /// <summary>选择列</summary>
        public String Selects { get; set; }
        #endregion

        #region 构造
        /// <summary>实例化数据库工作者</summary>
        public DataJob()
        {
            SupportPage = true;
        }
        #endregion

        #region 方法
        /// <summary>开始</summary>
        public override Boolean Start()
        {
            if (Active) return false;

            if (Factory == null) throw new ArgumentNullException(nameof(Factory));

            if (Field == null) Field = Factory.MasterTime;
            if (Field == null) throw new ArgumentNullException(nameof(Field));

            var job = Model;
            if (job.Step == 0) job.Step = 30;

            // 获取最小时间
            if (job.Start.Year < 2000) throw new InvalidOperationException("数据任务必须设置开始时间");

            return base.Start();
        }
        #endregion

        #region 数据抽取
        /// <summary>分批抽取数据，一个任务内多次调用</summary>
        /// <param name="ctx">上下文</param>
        /// <param name="set"></param>
        /// <returns></returns>
        protected override Object Fetch(JobContext ctx, ITask set)
        {
            if (set == null) throw new ArgumentNullException(nameof(set), "没有设置数据抽取配置");

            // 验证时间段
            var start = set.Start;
            var end = set.End;

            // 区间无效
            if (start >= end) return null;

            // 分批获取数据，如果没有取到，则结束
            var fi = Field;
            var exp = new WhereExpression();
            if (start > DateTime.MinValue && start < DateTime.MaxValue) exp &= fi >= start;
            if (end > DateTime.MinValue && end < DateTime.MaxValue) exp &= fi < end;

            if (!Where.IsNullOrEmpty()) exp &= Where;

            var list = Factory.FindAll(exp, OrderBy, Selects, set.Row, set.BatchSize);

            // 取到数据，需要滑动窗口
            if (list.Count > 0) set.Row += list.Count;

            return list;
        }
        #endregion

        #region 数据处理
        /// <summary>处理一批数据</summary>
        /// <param name="ctx">上下文</param>
        /// <returns></returns>
        protected override Int32 Execute(JobContext ctx)
        {
            var count = 0;
            foreach (var item in ctx.Data as IEnumerable)
            {
                try
                {
                    //ctx.Key = item + "";
                    ctx.Entity = item;

                    if (ProcessItem(ctx, item as IEntity)) count++;
                }
                catch (Exception ex)
                {
                    ctx.Error = ex;
                    if (!OnError(ctx)) throw;
                }
            }

            return count;
        }

        /// <summary>处理一个数据对象</summary>
        /// <param name="ctx">上下文</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual Boolean ProcessItem(JobContext ctx, IEntity entity) => true;
        #endregion
    }
}