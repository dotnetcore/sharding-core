using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Friday, 05 February 2021 15:21:04
    * @Email: 326308290@qq.com
    */
    public class VirtualDataSource : IVirtualDataSource
    {
        private readonly ConcurrentDictionary<Type, IVirtualDataSourceRoute> _dataSourceVirtualRoutes = new ConcurrentDictionary<Type, IVirtualDataSourceRoute>();

        private readonly ConcurrentDictionary<string, IPhysicDataSource> _physicDataSources =
            new ConcurrentDictionary<string, IPhysicDataSource>();

        public string DefaultDataSourceName { get; }
        public Type ShardingDbContextType { get; }

        public VirtualDataSource(Type shardingDbContextType,string defaultDataSourceName)
        {
            ShardingDbContextType = shardingDbContextType;
            DefaultDataSourceName = defaultDataSourceName??throw new ArgumentNullException(nameof(defaultDataSourceName));
        }



        public List<string> RouteTo(Type entityType,ShardingDataSourceRouteConfig routeRouteConfig)
        {
            var shardingEntityConfig = ShardingUtil.Parse(entityType);
            var virtualDataSourceRoute = GetRoute( entityType);

            if (routeRouteConfig.UseQueryable())
                return virtualDataSourceRoute.RouteWithWhere(routeRouteConfig.GetQueryable());
            if (routeRouteConfig.UsePredicate())
                return virtualDataSourceRoute.RouteWithWhere((IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(entityType), routeRouteConfig.UsePredicate()));
            object shardingKeyValue = null;
            if (routeRouteConfig.UseValue())
                shardingKeyValue = routeRouteConfig.GetShardingKeyValue();

            if (routeRouteConfig.UseEntity())
                shardingKeyValue = routeRouteConfig.GetShardingDataSource().GetPropertyValue(shardingEntityConfig.ShardingDataSourceField);

            if (shardingKeyValue != null)
            {
                var dataSourceName = virtualDataSourceRoute.RouteWithValue(shardingKeyValue);
                return new List<string>(1) { dataSourceName };
            }

            throw new NotImplementedException(nameof(ShardingDataSourceRouteConfig));
        }

        public IVirtualDataSourceRoute GetRoute(Type entityType)
        {
            if(!entityType.IsShardingDataSource())
                throw new InvalidOperationException(
                    $"entity type :[{entityType.FullName}] not impl [{nameof(IShardingDataSource)}]");

            if (!_dataSourceVirtualRoutes.TryGetValue(entityType, out var dataSourceVirtualRoute))
                throw new InvalidOperationException(
                    $"entity type :[{entityType.FullName}] not found virtual data source route");
            return dataSourceVirtualRoute;
        }

        public ISet<IPhysicDataSource> GetAllPhysicDataSources()
        {
            return _physicDataSources.Values.ToHashSet();
        }

        public IPhysicDataSource GetDefaultDataSource()
        {
            return GetPhysicDataSource(DefaultDataSourceName);
        }

        public IPhysicDataSource GetPhysicDataSource(string dataSourceName)
        {
            if (!_physicDataSources.TryGetValue(dataSourceName, out var physicDataSource))
                throw new InvalidOperationException($"not found  data source that name is :[{dataSourceName}]");

            return physicDataSource;
        }

        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
        {
            if (physicDataSource.IsDefault && physicDataSource.DataSourceName != DefaultDataSourceName)
                throw new InvalidOperationException($"default data source name:[{DefaultDataSourceName}],add physic default data source name:[{physicDataSource.DataSourceName}]");
            return _physicDataSources.TryAdd(physicDataSource.DataSourceName, physicDataSource);
        }
    }
}