using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Months;

namespace ShardingCore.Test2x.Shardings
{
    public class LogMonthLongvirtualRoute:AbstractSimpleShardingMonthKeyLongVirtualTableRoute<LogMonthLong>
    {
        protected override bool EnableHintRoute => true;
        //public override bool? EnableRouteParseCompileCache => true;

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override void Configure(EntityMetadataTableBuilder<LogMonthLong> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }
    }
}
