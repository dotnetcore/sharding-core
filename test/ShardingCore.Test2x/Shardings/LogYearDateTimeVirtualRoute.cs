using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Years;

namespace ShardingCore.Test2x.Shardings
{
    public class LogYearDateTimeVirtualRoute:AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute<LogYearDateTime>
    {
        protected override bool EnableHintRoute => true;
        //public override bool? EnableRouteParseCompileCache => true;

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            return new DateTime(2020, 1, 1);
        }
        public override void Configure(EntityMetadataTableBuilder<LogYearDateTime> builder)
        {
            builder.ShardingProperty(o => o.LogTime);
        }
    }
}
