using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Days;

namespace ShardingCore.Test2x.Shardings
{
    public class LogDayLongVirtualRoute:AbstractSimpleShardingDayKeyLongVirtualTableRoute<LogDayLong>
    {
        protected override bool EnableHintRoute => true;

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
    }
}
