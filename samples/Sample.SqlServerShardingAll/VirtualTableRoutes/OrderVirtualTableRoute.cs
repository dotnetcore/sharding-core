using System;
using Sample.SqlServerShardingAll.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Months;

namespace Sample.SqlServerShardingAll.VirtualTableRoutes
{
    public class OrderVirtualTableRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 1, 1);
        }

        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.CreationTime);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
}
