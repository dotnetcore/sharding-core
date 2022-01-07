﻿using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Extensions
{
    public static class ShardingEntityConfigOptionExtension
    {
        public static bool TryGetVirtualTableRoute<TEntity>(this IShardingEntityConfigOptions shardingEntityConfigOptions, out Type virtualTableRouteType) where TEntity:class
        {
            if (shardingEntityConfigOptions.HasVirtualTableRoute(typeof(TEntity)))
            {
                virtualTableRouteType = shardingEntityConfigOptions.GetVirtualTableRouteType(typeof(TEntity));
                return virtualTableRouteType != null;
            }

            virtualTableRouteType = null;
            return false;
        }
        public static bool TryGetVirtualDataSourceRoute<TEntity>(this IShardingEntityConfigOptions shardingEntityConfigOptions,out Type virtualDataSourceRouteType) where TEntity:class
        {
            if (shardingEntityConfigOptions.HasVirtualDataSourceRoute(typeof(TEntity)))
            {
                virtualDataSourceRouteType = shardingEntityConfigOptions.GetVirtualDataSourceRouteType(typeof(TEntity));
                return virtualDataSourceRouteType != null;
            }

            virtualDataSourceRouteType = null;
            return false;
        }
    }
}