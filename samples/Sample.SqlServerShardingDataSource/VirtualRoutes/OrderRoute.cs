using Sample.SqlServerShardingDataSource.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.SqlServerShardingDataSource.VirtualRoutes
{
    public class OrderRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
    {
        public OrderRoute() : base(2, 3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
