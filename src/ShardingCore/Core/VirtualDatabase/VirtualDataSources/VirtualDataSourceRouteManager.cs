using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    public class VirtualDataSourceRouteManager<TShardingDbContext> : IVirtualDataSourceRouteManager<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;
        private readonly ConcurrentDictionary<Type, IVirtualDataSourceRoute> _dataSourceVirtualRoutes = new ConcurrentDictionary<Type, IVirtualDataSourceRoute>();

        public VirtualDataSourceRouteManager(IEntityMetadataManager<TShardingDbContext> entityMetadataManager)
        {
            _entityMetadataManager = entityMetadataManager;
        }

        public IVirtualDataSourceRoute<TEntity> GetRoute<TEntity>() where TEntity : class
        {
            return (IVirtualDataSourceRoute<TEntity>)GetRoute(typeof(TEntity));
        }

        public IVirtualDataSourceRoute GetRoute(Type entityType)
        {
            if (!_entityMetadataManager.IsShardingDataSource(entityType))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not configure sharding data source");

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
        public bool AddVirtualDataSourceRoute(IVirtualDataSourceRoute virtualDataSourceRoute)
        {
            if (!virtualDataSourceRoute.EntityMetadata.IsShardingDataSource())
                throw new ShardingCoreInvalidOperationException($"{virtualDataSourceRoute.EntityMetadata.EntityType.FullName} should configure sharding data source");

            return _dataSourceVirtualRoutes.TryAdd(virtualDataSourceRoute.EntityMetadata.EntityType, virtualDataSourceRoute);
        }
    }
}
