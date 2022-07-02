using System;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Helpers;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Days;

namespace ShardingCore.Test2x.Shardings
{
    public class LogDayLongVirtualRoute:AbstractSimpleShardingDayKeyLongVirtualTableRoute<LogDayLong>
    {
        protected override bool EnableHintRoute => true;
        ////public override bool? EnableRouteParseCompileCache => true;

        public override void Configure(EntityMetadataTableBuilder<LogDayLong> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = GetBeginTime().Date;

            var tails = new List<string>();
            //提前创建表
            var nowTimeStamp = new DateTime(2021, 11, 20).Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var currentTimeStampLong = ShardingCoreHelper.ConvertDateTimeToLong(currentTimeStamp);
                var tail = TimeFormatToTail(currentTimeStampLong);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }
            return tails;
        }

    }
}
