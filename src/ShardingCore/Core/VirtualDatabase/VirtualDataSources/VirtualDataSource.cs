using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 15:21:04
    * @Email: 326308290@qq.com
    */
    public class VirtualDataSource<TShardingDbContext> : IVirtualDataSource<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly ConcurrentDictionary<Type, IVirtualDataSourceRoute> _dataSourceVirtualRoutes = new ConcurrentDictionary<Type, IVirtualDataSourceRoute>();

        private readonly ConcurrentDictionary<string, IPhysicDataSource> _physicDataSources =
            new ConcurrentDictionary<string, IPhysicDataSource>();

        public string DefaultDataSourceName { get; private set; }

        public VirtualDataSource(IEntityMetadataManager<TShardingDbContext> entityMetadataManager)
        {
            _entityMetadataManager = entityMetadataManager;
        }


        public List<string> RouteTo(Type entityType,ShardingDataSourceRouteConfig routeRouteConfig)
        {
            var virtualDataSourceRoute = GetRoute( entityType);

            if (routeRouteConfig.UseQueryable())
                return virtualDataSourceRoute.RouteWithPredicate(routeRouteConfig.GetQueryable(), true);
            if (routeRouteConfig.UsePredicate())
                return virtualDataSourceRoute.RouteWithPredicate((IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(entityType), routeRouteConfig.UsePredicate()), false);
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
            if(!_entityMetadataManager.IsShardingDataSource(entityType))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not configure sharding data source");

            if (!_dataSourceVirtualRoutes.TryGetValue(entityType, out var dataSourceVirtualRoute))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not found virtual data source route");
            return dataSourceVirtualRoute;
        }

        public ISet<IPhysicDataSource> GetAllPhysicDataSources()
        {
            return _physicDataSources.Select(o=>o.Value).ToHashSet();
        }

        public IPhysicDataSource GetDefaultDataSource()
        {
            return GetPhysicDataSource(DefaultDataSourceName);
        }

        public IPhysicDataSource GetPhysicDataSource(string dataSourceName)
        {
            Check.NotNull(dataSourceName, "data source name is null,plz confirm IShardingBootstrapper.Star()");
            if (!_physicDataSources.TryGetValue(dataSourceName, out var physicDataSource))
                throw new ShardingCoreInvalidOperationException($"not found  data source that name is :[{dataSourceName}]");

            return physicDataSource;
        }

        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
        {
            if (physicDataSource.IsDefault)
            {
                if (!string.IsNullOrWhiteSpace(DefaultDataSourceName))
                {
                    throw new ShardingCoreInvalidOperationException($"default data source name:[{DefaultDataSourceName}],add physic default data source name:[{physicDataSource.DataSourceName}]");
                }

                DefaultDataSourceName = physicDataSource.DataSourceName;
            }
            return _physicDataSources.TryAdd(physicDataSource.DataSourceName, physicDataSource);
        }

        public bool AddVirtualDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute)
        {
            if (!virtualDataSourceRoute.EntityMetadata.IsShardingDataSource())
                throw new ShardingCoreInvalidOperationException($"{virtualDataSourceRoute.EntityMetadata.EntityType.FullName} should configure sharding data source");

            return _dataSourceVirtualRoutes.TryAdd(virtualDataSourceRoute.EntityMetadata.EntityType, virtualDataSourceRoute);
        }

        public bool IsDefault(string dataSourceName)
        {
            if (string.IsNullOrWhiteSpace(DefaultDataSourceName))
            {
                throw new ShardingCoreInvalidOperationException("virtual data source not inited");
            }

            return DefaultDataSourceName== dataSourceName;
        }
    }
}