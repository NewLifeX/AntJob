using System;
using System.Collections.Generic;
using AntJob;
using HisData;
using NewLife.Security;
using XCode;

namespace HisAgent
{
    internal class BuildPatient : Handler
    {
        public BuildPatient()
        {
            var job = Job;
            job.Start = DateTime.Today;
            job.Step = 15;
        }

        protected override Int32 Execute(JobContext ctx)
        {
            // 随机造几个病人
            var count = Rand.Next(1, 9);

            var list = new List<ZYBH0>();
            for (var i = 0; i < count; i++)
            {
                var time = DateTime.Now.AddSeconds(Rand.Next(-30 * 24 * 3600, 0));
                var time2 = time.AddSeconds(Rand.Next(3600, 10 * 24 * 3600));
                var pi = new ZYBH0
                {
                    Bhid = Rand.Next(999999),
                    XM = Rand.NextString(8),
                    Ryrq = time,
                    Cyrq = time2,
                    Sfzh = Rand.NextString(18),
                    FB = Rand.NextString(6),
                    State = Rand.Next(8),
                    Flag = Rand.Next(2),
                };

                list.Add(pi);
            }
            list.Insert(true);

            // 成功处理数据量
            return count;
        }
    }
}