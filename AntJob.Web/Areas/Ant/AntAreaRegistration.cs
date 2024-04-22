using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.ViewModels;
using XCode;

namespace AntJob.Web.Areas.Ant;

/// <summary>蚂蚁调度</summary>
[DisplayName("蚂蚁调度")]
public class AntArea : AreaBase
{
    public AntArea() : base(nameof(AntArea).TrimEnd("Area")) { }

    static AntArea() => RegisterArea<AntArea>();
}

public class AntEntityController<T> : EntityController<T> where T : Entity<T>, new()
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        base.OnActionExecuting(filterContext);

        var appId = GetRequest("appId").ToInt(-1);
        if (appId > 0)
        {
            PageSetting.NavView = "_App_Nav";
            PageSetting.EnableNavbar = false;
        }
    }

    protected override FieldCollection OnGetFields(ViewKinds kind, Object model)
    {
        var fields = base.OnGetFields(kind, model);

        if (kind == ViewKinds.List)
        {
            var appId = GetRequest("appId").ToInt(-1);
            if (appId > 0) fields.RemoveField("AppName");
        }

        return fields;
    }
}