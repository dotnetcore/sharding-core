using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ShardingEnumerableQueries;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
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

        private readonly IPhysicDataSourcePool _physicDataSourcePool;

        public string DefaultDataSourceName { get; private set; }
        public string DefaultConnectionString { get; private set; }

        public VirtualDataSource(IEntityMetadataManager<TShardingDbContext> entityMetadataManager)
        {
            _entityMetadataManager = entityMetadataManager;
            _physicDataSourcePool = new PhysicDataSourcePool();
        }


        public List<string> RouteTo(Type entityType,ShardingDataSourceRouteConfig routeRouteConfig)
        {
            if (!_entityMetadataManager.IsShardingDataSource(entityType))
                return new List<string>(1) { DefaultDataSourceName };
            var virtualDataSourceRoute = GetRoute( entityType);

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
            if(!_entityMetadataManager.IsShardingDataSource(entityType))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not configure sharding data source");

            if (!_dataSourceVirtualRoutes.TryGetValue(entityType, out var dataSourceVirtualRoute))
                throw new ShardingCoreInvalidOperationException(
                    $"entity type :[{entityType.FullName}] not found virtual data source route");
            return dataSourceVirtualRoute;
        }
        /// <summary>
        /// 获取默认的数据源信息
        /// </summary>
        /// <returns></returns>
        public IPhysicDataSource GetDefaultDataSource()
        {
            return GetPhysicDataSource(DefaultDataSourceName);
        }
        /// <summary>
        /// 获取物理数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        public IPhysicDataSource GetPhysicDataSource(string dataSourceName)
        {
            Check.NotNull(dataSourceName, "data source name is null,plz confirm IShardingBootstrapper.Star()");
            var dataSource = _physicDataSourcePool.TryGet(dataSourceName);
            if (null== dataSource)
                throw new ShardingCoreNotFoundException($"data source:[{dataSourceName}]");

            return dataSource;
        }
        /// <summary>
        /// 获取所有的数据源名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllDataSourceNames()
        {
            return _physicDataSourcePool.GetAllDataSourceNames();
        }

        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreNotFoundException"></exception>
        public string GetConnectionString(string dataSourceName)
        {
            if (IsDefault(dataSourceName))
                return DefaultConnectionString;
            return GetPhysicDataSource(dataSourceName).ConnectionString;
        }
        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="physicDataSource"></param>
        /// <returns></returns>
        /// <exception cref="ShardingCoreInvalidOperationException">重复添加默认数据源</exception>
        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
        {
            if (physicDataSource.IsDefault)
            {
                if (!string.IsNullOrWhiteSpace(DefaultDataSourceName))
                {
                    throw new ShardingCoreInvalidOperationException($"default data source name:[{DefaultDataSourceName}],add physic default data source name:[{physicDataSource.DataSourceName}]");
                }
                DefaultDataSourceName = physicDataSource.DataSourceName;
                DefaultConnectionString = physicDataSource.ConnectionString;
            }

            return _physicDataSourcePool.TryAdd(physicDataSource);
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
        /// <summary>
        /// 是否是默认数据源
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public bool IsDefault(string dataSourceName)
        {
            return DefaultDataSourceName == dataSourceName;
        }
        /// <summary>
        /// 检查是否配置默认数据源和默认链接字符串
        /// </summary>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public void CheckVirtualDataSource()
        {
            if (string.IsNullOrWhiteSpace(DefaultDataSourceName))
                throw new ShardingCoreInvalidOperationException(
                    $"virtual data source not inited {nameof(DefaultDataSourceName)} in IShardingDbContext null");
            if (string.IsNullOrWhiteSpace(DefaultConnectionString))
                throw new ShardingCoreInvalidOperationException(
                    $"virtual data source not inited {nameof(DefaultConnectionString)} in IShardingDbContext null");
        }
    }
}