using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Days;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore6x.ShardingDbContexts
{
    internal class OrderVirtualTableRoute:AbstractSimpleShardingModKeyStringVirtualTableRoute<Order>
    {
        ////public override bool? EnableRouteParseCompileCache => true;

        public OrderVirtualTableRoute() : base(2, 5)
        {
        }
        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }

    }
}
