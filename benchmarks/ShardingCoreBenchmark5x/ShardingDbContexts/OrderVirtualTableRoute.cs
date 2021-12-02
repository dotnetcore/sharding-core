using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCoreBenchmark5x.ShardingDbContexts
{
    internal class OrderVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
    {
        public OrderVirtualTableRoute() : base(2, 5)
        {
        }
        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }

    }
}
