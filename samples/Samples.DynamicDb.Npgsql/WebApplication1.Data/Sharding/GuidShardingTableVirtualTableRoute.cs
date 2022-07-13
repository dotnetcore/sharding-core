using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;
using WebApplication1.Data.Models;

namespace WebApplication1.Data.Sharding
{

    public class GuidShardingTableVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<GuidShardingTable>
    {
        public GuidShardingTableVirtualTableRoute() : base(3, 6)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<GuidShardingTable> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }

}
