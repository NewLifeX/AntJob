using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>作业</summary>
    [DisplayName("作业")]
    [Area("Ant")]
    public class JobController : EntityController<Job>
    {
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

        public JobController() => PageSetting.EnableAdd = false;

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<Job> Search(Pager p)
        {
            var id = p["ID"].ToInt(-1);
            var appid = p["appid"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var mode = p["Mode"].ToInt(-1);

            return Job.Search(id, appid, start, end, mode, p["q"], p);
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
                var dt = Job.FindByID(id);
                if (dt == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

                dt.Enable = enable;
                dt.Save();
            }
            else
            {
                var ids = GetRequest("keys").SplitAsInt();

                foreach (var item in ids)
                {
                    var dt = Job.FindByID(item);
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
            var ids = GetRequest("keys").SplitAsInt();
            var st = GetRequest("sday").ToDateTime();
            var et = GetRequest("eday").ToDateTime();
            Parallel.ForEach(ids, k =>
            {
                var dt = Job.FindByID(k);
                dt?.ResetTime(days, st, et);
            });

            return JsonRefresh("操作成功！");
        }

        /// <summary>完全重置</summary>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult ResetOther()
        {
            var ids = GetRequest("keys").SplitAsInt();
            Parallel.ForEach(ids, k =>
            {
                var dt = Job.FindByID(k);
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

            var ids = GetRequest("keys").SplitAsInt();
            Parallel.ForEach(ids, k =>
            {
                var dt = Job.FindByID(k);
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
            var ids = GetRequest("keys").SplitAsInt();
            Parallel.ForEach(ids, k =>
            {
                var dt = Job.FindByID(k);
                if (dt != null)
                {
                    dt.Error = 0;
                    dt.Save();
                }
            });

            return JsonRefresh("操作成功！");
        }
    }
}