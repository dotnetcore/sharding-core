using Sample.SqlServerShardingTable.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.SqlServerShardingTable.VirtualRoutes
{
    public class SysUserVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUser>
    {
        public SysUserVirtualTableRoute() : base(2, 3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<SysUser> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
