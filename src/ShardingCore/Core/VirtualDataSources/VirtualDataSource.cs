using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Extensions;
using ShardingCore.Utils;

namespace ShardingCore.Core.VirtualDataSources
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 05 February 2021 15:21:04
* @Email: 326308290@qq.com
*/
    public class VirtualDataSource<T>:IVirtualDataSource<T> where T:class,IShardingDataSource
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataSourceVirtualRoute<T> _dataSourceVirtualRoute;
        public ShardingEntityBaseType ShardingEntityType { get; }

        public Type EntityType { get; }

        public VirtualDataSource(IServiceProvider serviceProvider, IDataSourceVirtualRoute<T> virtualRoute)
        {
            _serviceProvider = serviceProvider;
            _dataSourceVirtualRoute = virtualRoute;
            EntityType = typeof(T);
            ShardingEntityType = ShardingUtil.Parse(EntityType);
        }


        public List<string> RouteTo(VirutalDataSourceRouteConfig routeRouteConfig)
        {
            //获取所有的数据源
            var virtualDataSourceManager = _serviceProvider.GetService<IVirtualDataSourceManager>();
            var allShardingDataSourceConfigs = virtualDataSourceManager.GetAllShardingConnectKeys();

            if (routeRouteConfig.UseQueryable())
                return _dataSourceVirtualRoute.RouteWithWhere(allShardingDataSourceConfigs.ToList(), routeRouteConfig.GetQueryable());
            if (routeRouteConfig.UsePredicate())
                return _dataSourceVirtualRoute.RouteWithWhere(allShardingDataSourceConfigs.ToList(), new EnumerableQuery<T>((Expression<Func<T, bool>>) routeRouteConfig.GetPredicate()));
            object shardingKeyValue = null;
            if (routeRouteConfig.UseValue())
                shardingKeyValue = routeRouteConfig.GetShardingKeyValue();

            if (routeRouteConfig.UseEntity())
                shardingKeyValue = routeRouteConfig.GetShardingDataSource().GetPropertyValue(ShardingEntityType.ShardingDataSourceField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = _dataSourceVirtualRoute.RouteWithValue(allShardingDataSourceConfigs.ToList(), shardingKeyValue);
                return new List<string>(1) {routeWithValue};
            }

            throw new NotImplementedException(nameof(VirutalDataSourceRouteConfig));
        }

        IDataSourceVirtualRoute<T> IVirtualDataSource<T>.GetRoute()
        {
            return _dataSourceVirtualRoute;
        }

        public IDataSourceVirtualRoute GetRoute()
        {
            return _dataSourceVirtualRoute;
        }
    }
}