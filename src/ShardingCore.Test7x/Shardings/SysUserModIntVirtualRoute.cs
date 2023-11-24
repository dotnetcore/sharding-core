using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore.Test.Shardings
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
