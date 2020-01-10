using System;
using System.Collections.Generic;
using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>作业错误</summary>
    [AntArea]
    [DisplayName("作业错误")]
    public class JobErrorController : EntityController<JobError>
    {
        static JobErrorController() => MenuOrder = 60;

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<JobError> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var jobid = p["JobID"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var client = p["Client"];

            return JobError.Search(appid, jobid, client, start, end, p["q"], p);
        }
    }
}