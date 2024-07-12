using System;
using System.ComponentModel;
using AntJob;
using AntJob.Extensions;
using HisData;
using NewLife.Security;
using XCode;

namespace HisAgent;

[DisplayName("生产遗嘱")]
[Description("根据病人生成其对应的遗嘱")]
class BuildWill : DataHandler
{
    public BuildWill()
    {
        var job = Job;
        job.DataTime = DateTime.Today;
        job.Step = 30;
        job.BatchSize = 1000;
    }

    public override Boolean Start()
    {
        // 指定要抽取数据的实体类以及时间字段
        Factory = ZYBH0.Meta.Factory;
        Field = ZYBH0._.CreateTime;

        return base.Start();
    }

    public override Boolean ProcessItem(JobContext ctx, IEntity entity)
    {
        var pi = entity as ZYBH0;

        // 创建医嘱信息
        var will = new ZYBHYZ0
        {
            Bhid = pi.Bhid,
            Mgroupid = Rand.Next(9999),

            Kyzrq = pi.Ryrq.AddHours(1),
            Tyzrq = pi.Cyrq.AddHours(-3),
            Kyzys = Rand.NextString(8),

            State = pi.State,
        };

        will.Insert();

        return true;
    }
}