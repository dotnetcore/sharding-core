using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 15:43:09
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class VirtualDbContextExtension
    {
        public static string GetConnectKey<T>(this IVirtualDataSourceManager virtualDataSourceManager, T entity) where T : class
        {
            var type = entity.GetType();
            var connectKeys = virtualDataSourceManager.GetEntityTypeLinkedConnectKeys(type);
            if (connectKeys.IsEmpty())
                throw new ShardingCoreException($"entity:[{type}] connect not found");
            if (connectKeys.Count == 1)
                return connectKeys[0];
            if (!entity.IsShardingDataSource())
                throw new ShardingCoreException($"entity:[{type}] is not sharding data source and not found connect");
            return virtualDataSourceManager.GetVirtualDataSource(type)
                 .RouteTo(new VirutalDataSourceRouteConfig(shardingDataSource: entity as IShardingDataSource))[0];
        }
        public static List<string> GetConnectKeys<T>(this IVirtualDataSourceManager virtualDataSourceManager, Expression<Func<T, bool>> where) where T : class
        {
            var type = typeof(T);
            var connectKeys = virtualDataSourceManager.GetEntityTypeLinkedConnectKeys(type);
            if (connectKeys.IsEmpty())
                throw new ShardingCoreException($"entity:[{type}] connect not found");
         
            if (!type.IsShardingDataSource())
                throw new ShardingCoreException($"entity:[{type}] is not sharding data source and not found connect");
            return virtualDataSourceManager.GetVirtualDataSource(type)
                .RouteTo(new VirutalDataSourceRouteConfig(predicate: where));
        }
        public static List<string> GetConnectKeys<T>(this IVirtualDataSourceManager virtualDataSourceManager, IQueryable queryable) where T : class
        {
            var type = typeof(T);
            var connectKeys = virtualDataSourceManager.GetEntityTypeLinkedConnectKeys(type);
            if (connectKeys.IsEmpty())
                throw new ShardingCoreException($"entity:[{type}] connect not found");
         
            if (!type.IsShardingDataSource())
                throw new ShardingCoreException($"entity:[{type}] is not sharding data source and not found connect");
            return virtualDataSourceManager.GetVirtualDataSource(type)
                .RouteTo(new VirutalDataSourceRouteConfig(queryable: queryable));
        }
    }
}
