using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDataSources;
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
        public static IVirtualDataSource<TEntity> GetVirtualDataSource<TShardingDbContext,TEntity>(this IVirtualDataSourceManager virtualDataSourceManager)
            where TShardingDbContext:DbContext,IShardingDbContext
            where TEntity : class,IShardingDataSource
        {
            return (IVirtualDataSource<TEntity>)virtualDataSourceManager.GetVirtualDataSource(typeof(TShardingDbContext), typeof(TEntity));
        }
        public static string GetDataSourceName<TEntity>(this IVirtualDataSourceManager virtualDataSourceManager, Type shardingDbContextType, TEntity entity) where TEntity : class
        {
            return virtualDataSourceManager.GetPhysicDataSource(shardingDbContextType, entity).DSName;
        }
        public static IPhysicDataSource GetPhysicDataSource<TEntity>(this IVirtualDataSourceManager virtualDataSourceManager, Type shardingDbContextType, TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            if (!entity.IsShardingDataSource())
                return virtualDataSourceManager.GetDefaultDataSource(shardingDbContextType);
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(type);
            return virtualDataSource.RouteTo(
                new ShardingDataSourceRouteConfig(shardingDataSource: entity as IShardingDataSource))[0];
        }

        public static List<string> GetDataSourceNames<TEntity>(this IVirtualDataSourceManager virtualDataSourceManager, Type shardingDbContextType, Expression<Func<TEntity, bool>> where)
            where TEntity : class
        {
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(shardingDbContextType, typeof(TEntity));
            return virtualDataSource.RouteTo(new ShardingDataSourceRouteConfig(predicate: where)).Select(o => o.DSName)
                .ToList();
        }
    }
}
