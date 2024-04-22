using System.ComponentModel;
using AntJob.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Security;
using NewLife.Web;
using XCode.Membership;

namespace AntJob.Web.Areas.Ant.Controllers;

/// <summary>作业</summary>
[AntArea]
[DisplayName("作业")]
[Menu(0, false)]
public class JobController : AntEntityController<Job>
{
    static JobController()
    {
        LogOnChange = true;

        ListFields.RemoveCreateField().RemoveUpdateField();
        ListFields.AddListField("UpdateTime");
    }

    /// <summary>搜索数据集</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected override IEnumerable<Job> Search(Pager p)
    {
        PageSetting.EnableAdd = false;

        var id = p["ID"].ToInt(-1);
        var appid = p["appid"].ToInt(-1);
        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();
        var mode = p["Mode"].ToInt(-1);

        return Job.Search(id, appid, start, end, mode, p["q"], p);
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ResetTime(Int32 days = 0)
    {
        var ids = GetRequest("keys").SplitAsInt();
        var st = GetRequest("sday").ToDateTime();
        var et = GetRequest("eday").ToDateTime();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            dt?.ResetTime(days, st, et);
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>完全重置</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ResetOther()
    {
        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            dt?.ResetOther();
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>设置偏移</summary>
    /// <param name="offset">偏移</param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult SetOffset(Int32 offset)
    {
        if (offset < 0) offset = 15;

        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            if (dt != null)
            {
                dt.Offset = offset;
                dt.Save();
            }
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>清空错误数</summary>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult ClearError()
    {
        var ids = GetRequest("keys").SplitAsInt();
        Parallel.ForEach(ids, k =>
        {
            var dt = Job.FindByID(k);
            if (dt != null)
            {
                dt.Error = 0;
                dt.Save();
            }
        });

        return JsonRefresh("操作成功！");
    }

    /// <summary>克隆一个作业</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult Clone(Int32 id)
    {
        var job = Job.FindByID(id);
        if (job == null) return Index();

        // 拷贝一次对象，避免因为缓存等原因修改原来的数据
        job = job.Clone() as Job;

        // 随机名称，插入新行
        job.ID = 0;
        job.Name = Rand.NextString(8);
        job.Enable = false;
        job.Insert();

        // 跳转到编辑页，这里时候已经得到新的自增ID
        return Edit(job.ID + "");
    }
}