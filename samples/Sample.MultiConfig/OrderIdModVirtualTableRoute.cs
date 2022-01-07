using Sample.MultiConfig.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.MultiConfig
{
    public class OrderIdModVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
    {
        public OrderIdModVirtualTableRoute() : base(2, 5)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
