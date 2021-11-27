using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Days;

namespace ShardingCore.Test.Shardings
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
