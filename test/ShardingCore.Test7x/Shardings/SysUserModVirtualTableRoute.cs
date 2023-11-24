using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Test.Domain.Entities;
using ShardingCore.VirtualRoutes.Mods;

namespace ShardingCore.Test.Shardings
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 14 January 2021 15:39:27
    * @Email: 326308290@qq.com
    */
    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        protected override bool EnableHintRoute => true;
        //public override bool? EnableRouteParseCompileCache => true;

        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserMod> builder)
        {
            builder.ShardingProperty(o => o.Id);
            builder.TableSeparator("_");
        }
    }
}