using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes
{
    public class DataSourceRouteManager:IDataSourceRouteManager
    {
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly ConcurrentDictionary<Type, IVirtualDataSourceRoute> _dataSourceVirtualRoutes = new ConcurrentDictionary<Type, IVirtualDataSourceRoute>();

        public DataSourceRouteManager(IEntityMetadataManager entityMetadataManager,IVirtualDataSource virtualDataSource)
        {
            _entityMetadataManager = entityMetadataManager;
            _virtualDataSource = virtualDataSource;
        }
        public bool HasRoute(Type entityType)
        {
            return _dataSourceVirtualRoutes.ContainsKey(entityType);
        }

        public List<string> RouteTo(Type entityType, ShardingDataSourceRouteConfig routeRouteConfig)
        {
            if (!_entityMetadataManager.IsShardingDataSource(entityType))
                return new List<string>(1) { _virtualDataSource.DefaultDataSourceName };
            var virtualDataSourceRoute = GetRoute(entityType);

            if (routeRouteConfig.UseQueryable())
                return virtualDataSourceRoute.RouteWithPredicate(routeRouteConfig.GetQueryable(), true);
            if (routeRouteConfig.UsePredicate())
            {
                var shardingEmptyEnumerableQuery = (IShardingEmptyEnumerableQuery)Activator.CreateInstance(typeof(ShardingEmptyEnumerableQuery<>).MakeGenericType(entityType), routeRouteConfig.GetPredicate());
                return virtualDataSourceRoute.RouteWithPredicate(shardingEmptyEnumerableQuery.EmptyQueryable(), false);
            }
            object shardingKeyValue = null;
            if (routeRouteConfig.UseValue())
                shardingKeyValue = routeRouteConfig.GetShardingKeyValue();

            if (routeRouteConfig.UseEntity())
            {
                shardingKeyValue = routeRouteConfig.GetShardingDataSource().GetPropertyValue(virtualDataSourceRoute.EntityMetadata.ShardingDataSourceProperty.Name);
            }

            if (shardingKeyValue != null)
            {
                var dataSourceName = virtualDataSourceRoute.RouteWithValue(shardingKeyValue);
                return new List<string>(1) { dataSourceName };
            }

            throw new NotImplementedException(nameof(ShardingDataSourceRouteConfig));
        }

        public IVirtualDataSourceRoute GetRoute(Type entityType)
        {
            if (!_dataSourceVirtualRoutes.TryGetValue(entityType, out var dataSourceVirtualRoute))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not found virtual data source route");
            return dataSourceVirtualRoute;
        }
        /// <summary>
        /// 添加分库路由
        /// </summary>
        /// <param name="virtualDataSourceRoute"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">对象未配置分库</exception>
        public bool AddDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute)
        {
            if (!virtualDataSourceRoute.EntityMetadata.IsShardingDataSource())
                throw new ShardingCoreInvalidOperationException($"{virtualDataSourceRoute.EntityMetadata.EntityType.FullName} should configure sharding data source");

            return _dataSourceVirtualRoutes.TryAdd(virtualDataSourceRoute.EntityMetadata.EntityType, virtualDataSourceRoute);
        }
    }
}
