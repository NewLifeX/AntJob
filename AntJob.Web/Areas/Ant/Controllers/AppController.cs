using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.ViewModels;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用系统</summary>
[AntArea]
[DisplayName("应用系统")]
[Menu(100)]
public class AppController : AntEntityController<App>
{
    static AppController()
    {
        LogOnChange = true;

        ListFields.RemoveField("UpdateUserID");
        ListFields.RemoveCreateField().RemoveRemarkField();

        {
            var df = ListFields.GetField("Name") as ListField;
            df.Url = "/Ant/App/Detail?id={ID}";
            df.Target = "_blank";
        }
        {
            var df = ListFields.AddListField("Online", "UpdateUser");
            df.DisplayName = "在线";
            df.Url = "/Ant/AppOnline?appid={ID}";
        }
        {
            var df = ListFields.AddListField("Job", "UpdateUser");
            df.DisplayName = "作业";
            df.Url = "/Ant/Job?appid={ID}";
        }
        {
            var df = ListFields.AddListField("Task", "UpdateUser");
            df.DisplayName = "任务";
            df.Url = "/Ant/JobTask?appid={ID}";
        }
        {
            var df = ListFields.AddListField("Message", "UpdateUser");
            df.DisplayName = "消息";
            df.Url = "/Ant/AppMessage?appid={ID}";
        }
        {
            var df = ListFields.AddListField("Error", "UpdateUser");
            df.DisplayName = "错误";
            df.Url = "/Ant/JobError?appid={ID}";
        }
        {
            var df = ListFields.AddListField("History", "UpdateUser");
            df.DisplayName = "历史";
            df.Url = "/Ant/AppHistory?appid={ID}";
        }
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        var appId = GetRequest("Id").ToInt(-1);
        if (appId > 0)
        {
            PageSetting.NavView = "_App_Nav";
            PageSetting.EnableNavbar = false;
        }
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<App> Search(Pager p)
    {
        var id = p["id"].ToInt(-1);
        if (id <= 0) id = p["appId"].ToInt(-1);
        if (id > 0)
        {
            var entity = App.FindByID(id);
            if (entity != null) return [entity];
        }

        var category = p["category"];
        var enable = p["Enable"]?.ToBoolean();

        return App.Search(category, enable, p["q"], p);
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
        if (ids.Length == 0) return JsonRefresh("未选中项！");

        var now = DateTime.Now;
        foreach (var appid in ids)
        {
            // 清空作业
            var jobs = Job.FindAllByAppID2(appid);
            foreach (var job in jobs)
            {
                job.DataTime = new DateTime(now.Year, now.Month, 1);
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
}