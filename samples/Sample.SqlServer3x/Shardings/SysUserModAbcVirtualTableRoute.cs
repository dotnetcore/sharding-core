using Sample.SqlServer3x.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.SqlServer3x.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:39:27
* @Email: 326308290@qq.com
*/
    public class SysUserModAbcVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserModAbc>
    {
        public SysUserModAbcVirtualTableRoute() : base(2,3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserModAbc> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}