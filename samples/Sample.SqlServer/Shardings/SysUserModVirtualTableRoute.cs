using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sample.SqlServer.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Sharding.PaginationConfigurations;
using ShardingCore.VirtualRoutes;
using ShardingCore.VirtualRoutes.Abstractions;
using ShardingCore.VirtualRoutes.Days;
using ShardingCore.VirtualRoutes.Mods;
using ShardingCore.VirtualRoutes.Weeks;

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
    }
}