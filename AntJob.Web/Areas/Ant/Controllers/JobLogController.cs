using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>作业日志</summary>
    [DisplayName("作业日志")]
    public class JobLogController : EntityController<JobLog>
    {
        static JobLogController()
        {
            MenuOrder = 70;

            JobLog.Meta.Modules.Add<TimeModule>();
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<JobLog> Search(Pager p)
        {
            var id = p["id"].ToInt(-1);
            var jobid = p["JobID"].ToInt(-1);
            var appid = p["AppID"].ToInt(-1);
            var status = (JobStatus)p["Status"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var client = p["Client"];

            return JobLog.Search(id, appid, jobid, status, start, end, client, p["q"], p);
        }

        /// <summary>修改状态</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EntityAuthorize(PermissionFlags.Update)]
        public ActionResult Set(Int32 id = 0)
        {
            if (id > 0)
            {
                var dt = JobLog.FindByID(id);
                if (dt == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

                dt.Status = JobStatus.取消;
                dt.Row = 0;
                if (dt.Times >= 10)
                {
                    dt.Times = 0;
                }

                dt.Save();
            }
            else
            {
                var ids = Request["keys"].SplitAsInt(",");

                foreach (var item in ids)
                {
                    var dt = JobLog.FindByID(item);
                    if (dt != null)
                    {
                        dt.Status = JobStatus.取消;
                        dt.Row = 0;
                        if (dt.Times >= 10)
                        {
                            dt.Times = 0;
                        }
                        dt.Save();
                    }
                }
            }
            return JsonRefresh("操作成功！");
        }
    }
}