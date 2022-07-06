using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
    public class TableRouteManager : ITableRouteManager
    {
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IDataSourceRouteManager _dataSourceRouteManager;
        private readonly ConcurrentDictionary<Type, IVirtualTableRoute> _tableRoutes = new();

        public TableRouteManager(IVirtualDataSource virtualDataSource,IDataSourceRouteManager dataSourceRouteManager)
        {
            _virtualDataSource = virtualDataSource;
            _dataSourceRouteManager = dataSourceRouteManager;
        }
        public bool HasRoute(Type entityType)
        {
            return _tableRoutes.ContainsKey(entityType);
        }

        public IVirtualTableRoute GetRoute(Type entityType)
        {
            if (!_tableRoutes.TryGetValue(entityType, out var tableRoute))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not found table route");
            return tableRoute;
        }

        public List<IVirtualTableRoute> GetRoutes()
        {
            return _tableRoutes.Values.ToList();
        }

        public bool AddRoute(IVirtualTableRoute route)
        {
            if (!route.EntityMetadata.IsShardingTable())
                throw new ShardingCoreInvalidOperationException(
                    $"{route.EntityMetadata.EntityType.FullName} should configure sharding table");

            return _tableRoutes.TryAdd(route.EntityMetadata.EntityType, route);
        }

        public List<TableRouteUnit> RouteTo(Type entityType, ShardingTableRouteConfig shardingTableRouteConfig)
        {
            return RouteTo(entityType, _virtualDataSource.DefaultDataSourceName, shardingTableRouteConfig);
        }

        public List<TableRouteUnit> RouteTo(Type entityType, string dataSourceName, ShardingTableRouteConfig shardingTableRouteConfig)
        {
            var dataSourceRouteResult = new DataSourceRouteResult(_virtualDataSource.DefaultDataSourceName);
            return RouteTo(entityType, dataSourceRouteResult, shardingTableRouteConfig);
        }

        public List<TableRouteUnit> RouteTo(Type entityType, DataSourceRouteResult dataSourceRouteResult,
            ShardingTableRouteConfig tableRouteConfig)
        {
            var route = GetRoute(entityType);
            if (tableRouteConfig.UseQueryable())
                return route.RouteWithPredicate(dataSourceRouteResult, tableRouteConfig.GetQueryable(), true);
            if (tableRouteConfig.UsePredicate())
            {
                var shardingEmptyEnumerableQuery = (IShardingEmptyEnumerableQuery)Activator.CreateInstance(
                    typeof(ShardingEmptyEnumerableQuery<>).GetGenericType0(entityType),
                    tableRouteConfig.GetPredicate());

                return route.RouteWithPredicate(dataSourceRouteResult, shardingEmptyEnumerableQuery!.EmptyQueryable(),
                    false);
            }

            object shardingKeyValue = null;
            if (tableRouteConfig.UseValue())
                shardingKeyValue = tableRouteConfig.GetShardingKeyValue();

            if (tableRouteConfig.UseEntity())
                shardingKeyValue = tableRouteConfig.GetShardingEntity()
                    .GetPropertyValue(route.EntityMetadata.ShardingTableProperty.Name);

            if (shardingKeyValue == null)
                throw new ShardingCoreException(" route entity queryable or sharding key value is null ");
            var shardingRouteUnit = route.RouteWithValue(dataSourceRouteResult, shardingKeyValue);
            return new List<TableRouteUnit>(1) { shardingRouteUnit };
        }
    }
}