using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Web;
using XCode;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>应用系统</summary>
[AntArea]
[DisplayName("应用系统")]
[Menu(100)]
public class AppController : EntityController<App>
{
    static AppController()
    {
        ListFields.RemoveField("UpdateUserID");
        ListFields.RemoveCreateField().RemoveRemarkField();

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

    protected override Boolean Valid(App entity, DataObjectMethodType type, Boolean post)
    {
        if (!post) return base.Valid(entity, type, post);

        var act = type switch
        {
            DataObjectMethodType.Update => "修改",
            DataObjectMethodType.Insert => "添加",
            DataObjectMethodType.Delete => "删除",
            _ => type + "",
        };

        // 必须提前写修改日志，否则修改后脏数据失效，保存的日志为空
        if (type == DataObjectMethodType.Update && (entity as IEntity).HasDirty)
            LogProvider.Provider.WriteLog(act, entity);

        var err = "";
        try
        {
            return base.Valid(entity, type, post);
        }
        catch (Exception ex)
        {
            err = ex.Message;
            throw;
        }
        finally
        {
            LogProvider.Provider.WriteLog(act, entity, err);
        }
    }
}