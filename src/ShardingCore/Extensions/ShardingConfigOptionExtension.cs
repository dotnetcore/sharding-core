using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Extensions
{
    public static class ShardingConfigOptionExtension
    {
        public static bool TryGetVirtualTableRoute<TEntity>(this IShardingConfigOption shardingConfigOption,out Type virtualTableRouteType) where TEntity:class
        {
            if (shardingConfigOption.HasVirtualTableRoute(typeof(TEntity)))
            {
                virtualTableRouteType = shardingConfigOption.GetVirtualTableRouteType(typeof(TEntity));
                return virtualTableRouteType != null;
            }

            virtualTableRouteType = null;
            return false;
        }
        public static bool TryGetVirtualDataSourceRoute<TEntity>(this IShardingConfigOption shardingConfigOption,out Type virtualDataSourceRouteType) where TEntity:class
        {
            if (shardingConfigOption.HasVirtualDataSourceRoute(typeof(TEntity)))
            {
                virtualDataSourceRouteType = shardingConfigOption.GetVirtualDataSourceRouteType(typeof(TEntity));
                return virtualDataSourceRouteType != null;
            }

            virtualDataSourceRouteType = null;
            return false;
        }
    }
}
