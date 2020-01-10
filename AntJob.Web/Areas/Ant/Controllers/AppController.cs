using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers
{
    /// <summary>应用系统</summary>
    [AntArea]
    [DisplayName("应用系统")]
    public class AppController : EntityController<App>
    {
        static AppController()
        {
            MenuOrder = 100;

            App.Meta.Modules.Add<UserModule>();
            App.Meta.Modules.Add<TimeModule>();
            App.Meta.Modules.Add<IPModule>();
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<App> Search(Pager p)
        {
            var id = p["id"].ToInt(-1);
            if (id > 0)
            {
                var list = new List<App>();
                var entity = App.FindByID(id);
                if (entity != null) list.Add(entity);

                return list;
            }

            return App.Search(p["category"], p["Enable"]?.ToBoolean(), p["q"], p);
        }

        protected override Int32 OnUpdate(App entity)
        {
            entity.JobCount = Job.FindCountByAppID(entity.ID);

            return base.OnUpdate(entity);
        }

        /// <summary>
        /// 重置应用
        /// 清空作业、作业项、统计、错误,开始时间设为本月一号
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetApp()
        {
            var ids = GetRequest("keys").SplitAsInt();
            if (!ids.Any()) return JsonRefresh("未选中项！");

            var now = DateTime.Now;
            foreach (var appid in ids)
            {
                // 清空作业
                var jobs = Job.FindAllByAppID2(appid);
                foreach (var job in jobs)
                {
                    job.Start = new DateTime(now.Year, now.Month, 1);
                    job.ResetOther();
                }

                // 清空日志
                var jobItems = JobTask.FindAllByAppID(appid);
                foreach (var jobItem in jobItems)
                {
                    jobItem.Delete();
                }

                // 清空错误
                JobError.DeleteByAppId(appid);
            }

            return JsonRefresh("操作完毕！");
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
                var dt = App.FindByID(id);
                if (dt == null) throw new ArgumentNullException(nameof(id), "找不到任务 " + id);

                dt.Enable = enable;
                dt.Save();
            }
            else
            {
                var ids = GetRequest("keys").SplitAsInt();

                foreach (var item in ids)
                {
                    var dt = App.FindByID(item);
                    if (dt != null && dt.Enable != enable)
                    {
                        dt.Enable = enable;
                        dt.Save();
                    }
                }
            }
            return JsonRefresh("操作成功！");
        }
    }
}