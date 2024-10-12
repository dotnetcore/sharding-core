using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.MySql.Shardings
{
    public class GroupEntityRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<GroupEntity>
    {
        public GroupEntityRoute() : base(2, 3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<GroupEntity> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}

