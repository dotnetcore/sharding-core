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
    public class VirtualDataSource<T>:IVirtualDataSource<T> where T:class
    {
        private readonly IVirtualDataSourceRoute<T> _dataSourceVirtualRoute;
        public ShardingEntityConfig ShardingEntityType { get; }

        public Type EntityType { get; }
        private readonly ConcurrentDictionary<IPhysicDataSource, object> _physicDataSources = new ConcurrentDictionary<IPhysicDataSource, object>();


        private readonly ConcurrentDictionary<string, IVirtualTable> _virtualTables =
            new ConcurrentDictionary<string, IVirtualTable>();

        public VirtualDataSource(IVirtualDataSourceRoute<T> virtualRoute)
        {
            _dataSourceVirtualRoute = virtualRoute;
            EntityType = typeof(T);
            ShardingEntityType = ShardingUtil.Parse(EntityType);
        }


        public List<IPhysicDataSource> RouteTo(ShardingDataSourceRouteConfig routeRouteConfig)
        {

            if (routeRouteConfig.UseQueryable())
                return _dataSourceVirtualRoute.RouteWithWhere(GetAllPhysicDataSources(), routeRouteConfig.GetQueryable());
            if (routeRouteConfig.UsePredicate())
                return _dataSourceVirtualRoute.RouteWithWhere(GetAllPhysicDataSources(), new EnumerableQuery<T>((Expression<Func<T, bool>>) routeRouteConfig.GetPredicate()));
            object shardingKeyValue = null;
            if (routeRouteConfig.UseValue())
                shardingKeyValue = routeRouteConfig.GetShardingKeyValue();

            if (routeRouteConfig.UseEntity())
                shardingKeyValue = routeRouteConfig.GetShardingDataSource().GetPropertyValue(ShardingEntityType.ShardingDataSourceField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = _dataSourceVirtualRoute.RouteWithValue(GetAllPhysicDataSources(), shardingKeyValue);
                return new List<IPhysicDataSource>(1) {routeWithValue};
            }

            throw new NotImplementedException(nameof(ShardingDataSourceRouteConfig));
        }

        IVirtualDataSourceRoute<T> IVirtualDataSource<T>.GetRoute()
        {
            return _dataSourceVirtualRoute;
        }

        public IVirtualDataSourceRoute GetRoute()
        {
            return _dataSourceVirtualRoute;
        }

        public ISet<IPhysicDataSource> GetAllPhysicDataSources()
        {
            return _physicDataSources.Keys.ToHashSet();
        }

        public bool AddPhysicDataSource(IPhysicDataSource physicDataSource)
        {
            if (physicDataSource.EntityType != EntityType)
                throw new InvalidOperationException($"virtual data source entity type :[{EntityType.FullName}] physic data source entity type:[{physicDataSource.EntityType.FullName}]");
            return _physicDataSources.TryAdd(physicDataSource, null);
        }

        public bool AddVirtualTable(string dsname, IVirtualTable virtualTable)
        {
            if (virtualTable.EntityType != EntityType)
                throw new InvalidOperationException($"virtual data source entity:{EntityType.FullName},virtual table entity:{virtualTable.EntityType.FullName}");
            if (_physicDataSources.Keys.All(o => o.DSName != dsname))
                throw new InvalidOperationException($"data source name:[{dsname}] not found virtual data source");
            return _virtualTables.TryAdd(dsname, virtualTable);
        }

        public ISet<IVirtualTable> GetVirtualTables()
        {
            return _virtualTables.Values.ToHashSet();
        }
    }
}