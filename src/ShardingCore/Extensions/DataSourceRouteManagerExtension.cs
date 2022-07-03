using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 16:42:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class DataSourceRouteManagerExtension
    {
        
        public static string GetDataSourceName<TEntity>(this IDataSourceRouteManager dataSourceRouteManager,TEntity entity)where TEntity : class
        {

            return dataSourceRouteManager.RouteTo(entity.GetType(),
                new ShardingDataSourceRouteConfig(shardingDataSource: entity))[0];
        }

        public static List<string> GetDataSourceNames<TEntity>(this IDataSourceRouteManager dataSourceRouteManager, Expression<Func<TEntity, bool>> where)
            where TEntity : class
        {
            return dataSourceRouteManager.RouteTo(typeof(TEntity),new ShardingDataSourceRouteConfig(predicate: where))
                .ToList();
        }
        public static string GetDataSourceName<TEntity>(this IDataSourceRouteManager dataSourceRouteManager, object shardingKeyValue) where TEntity : class
        {
            return dataSourceRouteManager.RouteTo(typeof(TEntity),
                new ShardingDataSourceRouteConfig(shardingKeyValue:shardingKeyValue))[0];
        }
    }
}
