using System;
using System.Collections.Generic;
using System.ComponentModel;
using AntJob.Data.Entity;
using NewLife.Cube;
using NewLife.Web;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>应用配置</summary>
    [AntArea]
    [DisplayName("应用配置")]
    public class AppConfigController : EntityController<AppConfig>
    {
        static AppConfigController()
        {
            MenuOrder = 83;

            AppConfig.Meta.Table.DataTable.InsertOnly = true;
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<AppConfig> Search(Pager p)
        {
            var appid = p["appid"].ToInt(-1);
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();

            return AppConfig.Search(appid, start, end, p["q"], p);
        }
    }
}