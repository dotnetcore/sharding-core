using ShardingCore.Core.ShardingConfigurations.Abstractions;
using System;

namespace ShardingCore.Extensions
{
    public static class ShardingEntityConfigOptionExtension
    {
        public static bool TryGetVirtualTableRoute<TEntity>(this IShardingRouteConfigOptions shardingRouteConfigOptions, out Type virtualTableRouteType) where TEntity:class
        {
            if (shardingRouteConfigOptions.HasVirtualTableRoute(typeof(TEntity)))
            {
                virtualTableRouteType = shardingRouteConfigOptions.GetVirtualTableRouteType(typeof(TEntity));
                return virtualTableRouteType != null;
            }

            virtualTableRouteType = null;
            return false;
        }
        public static bool TryGetVirtualDataSourceRoute<TEntity>(this IShardingRouteConfigOptions shardingRouteConfigOptions,out Type virtualDataSourceRouteType) where TEntity:class
        {
            if (shardingRouteConfigOptions.HasVirtualDataSourceRoute(typeof(TEntity)))
            {
                virtualDataSourceRouteType = shardingRouteConfigOptions.GetVirtualDataSourceRouteType(typeof(TEntity));
                return virtualDataSourceRouteType != null;
            }

            virtualDataSourceRouteType = null;
            return false;
        }
    }
}
