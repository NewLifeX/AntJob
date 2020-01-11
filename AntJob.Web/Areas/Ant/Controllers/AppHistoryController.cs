using System;
using System.Collections.Generic;
using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>应用历史</summary>
    [AntArea]
    [DisplayName("应用历史")]
    public class AppHistoryController : EntityController<AppHistory>
    {
        static AppHistoryController()
        {
            MenuOrder = 85;

            AppOnline.Meta.Table.DataTable.InsertOnly = true;
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<AppHistory> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var act = p["action"];
            var success = p["success"]?.ToBoolean();
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();

            return AppHistory.Search(appid, act, success, start, end, p["q"], p);
        }
    }
}