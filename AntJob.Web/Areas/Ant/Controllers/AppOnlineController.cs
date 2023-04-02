using System;
using System.Collections.Generic;
using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>应用在线</summary>
    [AntArea]
    [DisplayName("应用在线")]
    public class AppOnlineController : EntityController<AppOnline>
    {
        static AppOnlineController()
        {
            //MenuOrder = 90;

            AppOnline.Meta.Table.DataTable.InsertOnly = true;
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<AppOnline> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();

            return AppOnline.Search(appid, start, end, p["q"], p);
        }
    }
}