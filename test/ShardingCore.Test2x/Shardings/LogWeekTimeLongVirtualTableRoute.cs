using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Weeks;

namespace ShardingCore.Test2x.Shardings
{
    public class LogWeekTimeLongVirtualTableRoute : AbstractSimpleShardingWeekKeyLongVirtualTableRoute<LogWeekTimeLong>
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

        public override void Configure(EntityMetadataTableBuilder<LogWeekTimeLong> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }
    }
}
