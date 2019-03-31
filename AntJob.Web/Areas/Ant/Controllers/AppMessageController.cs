using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>应用消息</summary>
    [DisplayName("应用消息")]
    public class AppMessageController : EntityController<AppMessage>
    {
        static AppMessageController() => MenuOrder = 49;

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<AppMessage> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var JobID = p["JobID"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();
            var status = p["Status"];

            return AppMessage.Search(appid, JobID, start, end, p["q"], p);
        }
    }
}