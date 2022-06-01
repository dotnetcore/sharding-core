using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test2x.Domain.Entities;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore.Test2x.Shardings
{
    public class SysUserModIntVirtualRoute:AbstractSimpleShardingModKeyIntVirtualTableRoute<SysUserModInt>
    {
        protected override bool EnableHintRoute => true;
        //public override bool? EnableRouteParseCompileCache => true;

        public SysUserModIntVirtualRoute() : base(2, 3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserModInt> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}
