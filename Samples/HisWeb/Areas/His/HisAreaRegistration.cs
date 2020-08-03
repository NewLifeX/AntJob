using System;
using System.ComponentModel;
using NewLife;
using NewLife.Cube;

namespace HisWeb.Areas.His
{
    [DisplayName("医院管理")]
    public class HisArea : AreaBase
    {
        public HisArea() : base(nameof(HisArea).TrimEnd("Area")) { }

        static HisArea() => RegisterArea<HisArea>();
    }
}