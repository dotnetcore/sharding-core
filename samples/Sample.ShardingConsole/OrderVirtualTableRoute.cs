using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.ShardingConsole;

public class OrderVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
{
    public OrderVirtualTableRoute() : base(2, 3)
    {
    }

    public override void Configure(EntityMetadataTableBuilder<Order> builder)
    {
        builder.ShardingProperty(o => o.Id);
        builder.AutoCreateTable(null);
        builder.TableSeparator("_");
    }
}