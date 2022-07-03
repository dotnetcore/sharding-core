using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.SqlServer.Shardings
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 14 January 2021 15:39:27
    * @Email: 326308290@qq.com
    */
    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        /// <summary>
        /// 开启提示路由
        /// </summary>
        protected override bool EnableHintRoute => true;
        /// <summary>
        /// 开启断言路由
        /// </summary>
        protected override bool EnableAssertRoute => true;

        public SysUserModVirtualTableRoute() : base(2, 3)
        {
        }

        public override IPaginationConfiguration<SysUserMod> CreatePaginationConfiguration()
        {
            return new SysUserModPaginationConfiguration();
        }

        public override void Configure(EntityMetadataTableBuilder<SysUserMod> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }
    }
}