using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using AntJob.Data;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>作业任务</summary>
    [DisplayName("作业任务")]
    public class JobTaskController : EntityController<JobTask>
    {
        static JobTaskController()
        {
            MenuOrder = 70;

            JobTask.Meta.Modules.Add<TimeModule>();
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<JobTask> Search(Pager p)
        {
            var id = p["id"].ToInt(-1);
            var jobid = p["JobID"].ToInt(-1);
            var appid = p["AppID"].ToInt(-1);
            var status = (JobStatus)p["Status"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var client = p["Client"];

            return JobTask.Search(id, appid, jobid, status, start, end, client, p["q"], p);
        }

        /// <summary>修改状态</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult Set(Int32 id = 0)
        {
            if (id > 0)
            {
                var dt = JobTask.FindByID(id);
                if (dt == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

                dt.Status = JobStatus.取消;
                if (dt.Times >= 10) dt.Times = 0;

                dt.Save();
            }
            else
            {
                var ids = Request["keys"].SplitAsInt(",");

                foreach (var item in ids)
                {
                    var dt = JobTask.FindByID(item);
                    if (dt != null)
                    {
                        dt.Status = JobStatus.取消;
                        if (dt.Times >= 10) dt.Times = 0;

                        dt.Save();
                    }
                }
            }
            return JsonRefresh("操作成功！");
        }
    }
}