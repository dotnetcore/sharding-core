using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Weeks;

namespace ShardingCore.Test.Shardings
{
    public class LogWeekDateTimeVirtualTableRoute:AbstractSimpleShardingWeekKeyDateTimeVirtualTableRoute<LogWeekDateTime>
    {
        //public override bool? EnableRouteParseCompileCache => true;
        protected override bool EnableHintRoute => true;

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override void Configure(EntityMetadataTableBuilder<LogWeekDateTime> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }
    }
}
