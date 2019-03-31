using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Mvc;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;
using XCode;
using XCode.Membership;
using JobX = AntJob.Data.Entity.Job;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>作业</summary>
    [DisplayName("作业")]
    public class JobController : EntityController<JobX>
    {
        public JobController()
        {
            PageSetting.EnableAdd = false;
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<JobX> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var ID = p["ID"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var mode = p["Mode"].ToInt(-1);

            return JobX.Search(ID, appid, start, end, mode, p["q"], p);
        }

        static JobController()
        {
            var list = ListFields;
            list.RemoveField("Type");
            list.RemoveField("CreateUserID");
            list.RemoveField("CreateTime");
            list.RemoveField("CreateIP");
            list.RemoveField("UpdateUserID");
            list.RemoveField("UpdateIP");

            MenuOrder = 80;
        }

        /// <summary>启用禁用任务</summary>
        /// <param name="id"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult Set(Int32 id = 0, Boolean enable = true)
        {
            if (id > 0)
            {
                var dt = JobX.FindByID(id);
                if (dt == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

                dt.Enable = enable;
                dt.Save();
            }
            else
            {
                var ids = Request["keys"].SplitAsInt(",");

                foreach (var item in ids)
                {
                    var dt = JobX.FindByID(item);
                    if (dt != null && dt.Enable != enable)
                    {
                        dt.Enable = enable;
                        dt.Save();
                    }
                }
            }
            return JsonRefresh("操作成功！");
        }

        /// <summary>
        /// 重置时间
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult ResetTime(Int32 days = 0)
        {
            var ids = Request["keys"].SplitAsInt(",");
            var st = Request["sday"].ToDateTime();
            var et = Request["eday"].ToDateTime();
            Parallel.ForEach(ids, k =>
            {
                var dt = JobX.FindByID(k);
                dt?.ResetTime(days, st, et);
            });

            return JsonRefresh("操作成功！");
        }

        /// <summary>完全重置</summary>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult AllReset()
        {
            var ids = Request["keys"].SplitAsInt(",");
            Parallel.ForEach(ids, k =>
            {
                var dt = JobX.FindByID(k);
                dt?.ResetOther();
            });

            return JsonRefresh("操作成功！");
        }

        /// <summary>设置偏移</summary>
        /// <param name="offset">偏移</param>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult SetOffset(Int32 offset)
        {
            if (offset < 0) offset = 15;

            var ids = Request["keys"].SplitAsInt(",");
            Parallel.ForEach(ids, k =>
            {
                var dt = JobX.FindByID(k);
                if (dt != null)
                {
                    dt.Offset = offset;
                    dt.Save();
                }
            });

            return JsonRefresh("操作成功！");
        }

        /// <summary>清空错误数</summary>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult ClearError()
        {
            var ids = Request["keys"].SplitAsInt(",");
            Parallel.ForEach(ids, k =>
            {
                var dt = JobX.FindByID(k);
                if (dt != null)
                {
                    dt.Error = 0;
                    dt.Save();
                }
            });

            return JsonRefresh("操作成功！");
        }

        /// <summary>清空错误项</summary>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult ClearErrorItem()
        {
            var ids = Request["keys"].SplitAsInt(",");
            Parallel.ForEach(ids, k =>
            {
                var dt = JobError.FindAllByJobId(k);
                if (dt.Count < 20000)
                {
                    dt.Delete(true);
                }

                var dtt = JobX.FindByID(k);
                if (dtt != null)
                {
                    dtt.Error = 0;
                    dtt.Save();
                }
            });

            return JsonRefresh("操作成功！");
        }
    }
}