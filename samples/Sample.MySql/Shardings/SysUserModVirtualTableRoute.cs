using Sample.MySql.Domain.Entities;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.VirtualRoutes.Mods;

namespace Sample.MySql.Shardings
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 14 January 2021 15:39:27
* @Email: 326308290@qq.com
*/
    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }


        public override void Configure(EntityMetadataTableBuilder<SysUserMod> builder)
        {
            builder.ShardingProperty(o => o.Id);
        }

        // protected override List<TableRouteUnit> AfterShardingRouteUnitFilter(DataSourceRouteResult dataSourceRouteResult, List<TableRouteUnit> shardingRouteUnits)
        // {
        //     //拦截
        //     if (shardingRouteUnits.Count > 10)
        //     {
        //         return shardingRouteUnits.Take(10).ToList();
        //     }
        //     return base.AfterShardingRouteUnitFilter(dataSourceRouteResult, shardingRouteUnits);
        // }
    }
}