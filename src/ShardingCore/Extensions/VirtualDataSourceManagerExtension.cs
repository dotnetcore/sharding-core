using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 16:42:59
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class VirtualDataSourceManagerExtension
    {
        public static string GetDataSourceName<TEntity>(this IVirtualDataSourceManager virtualDataSourceManager,TEntity entity) where TEntity : class
        {
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource();
            if (!entity.IsShardingDataSource())
                return virtualDataSource.DefaultDataSourceName;

            return virtualDataSource.RouteTo(typeof(TEntity),
                new ShardingDataSourceRouteConfig(shardingDataSource: entity as IShardingDataSource))[0];
        }

        public static List<string> GetDataSourceNames<TEntity>(this IVirtualDataSourceManager virtualDataSourceManager,Expression<Func<TEntity, bool>> where)
            where TEntity : class
        {
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource();
            return virtualDataSource.RouteTo(typeof(TEntity),new ShardingDataSourceRouteConfig(predicate: where))
                .ToList();
        }
    }
}
