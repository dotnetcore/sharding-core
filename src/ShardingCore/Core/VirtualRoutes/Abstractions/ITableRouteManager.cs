using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
    
    public interface ITableRouteManager
    {
        bool HasRoute(Type entityType);
        IVirtualTableRoute GetRoute(Type entityType);
        List<IVirtualTableRoute> GetRoutes();
        bool AddRoute(IVirtualTableRoute route);

        List<ShardingRouteUnit> RouteTo(Type entityType,
            ShardingTableRouteConfig shardingTableRouteConfig);
        List<ShardingRouteUnit> RouteTo(Type entityType,DataSourceRouteResult dataSourceRouteResult,
            ShardingTableRouteConfig shardingTableRouteConfig);
    }
}
