using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core.PhysicDataSources;
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
        private readonly List<IPhysicDataSource> _physicDataSources;
        private readonly IVirtualDataSourceRoute<T> _virtualDataSourceRoute;
        public ShardingEntityBaseType ShardingEntityType { get; }

        public Type EntityType { get; }

        public VirtualDataSource(IServiceProvider serviceProvider)
        {
            _physicDataSources = new List<IPhysicDataSource>();
            _virtualDataSourceRoute = serviceProvider.GetService<IVirtualDataSourceRoute<T>>() ?? throw new ArgumentNullException($"{typeof(T)}");
            EntityType = typeof(T);
            ShardingEntityType = ShardingUtil.Parse(EntityType);
        }

        public List<IPhysicDataSource> GetAllDataSources()
        {
            return _physicDataSources;
        }

        public List<IPhysicDataSource> RouteTo(VirutalDataSourceConfig routeConfig)
        {
            if (routeConfig.UseQueryable())
                return _virtualDataSourceRoute.RouteWithWhere(_physicDataSources, routeConfig.GetQueryable());
            if (routeConfig.UsePredicate())
                return _virtualDataSourceRoute.RouteWithWhere(_physicDataSources, new EnumerableQuery<T>((Expression<Func<T, bool>>) routeConfig.GetPredicate()));
            object shardingKeyValue = null;
            if (routeConfig.UseValue())
                shardingKeyValue = routeConfig.GetShardingKeyValue();

            if (routeConfig.UseEntity())
                shardingKeyValue = routeConfig.GetShardingDataSource().GetPropertyValue(ShardingEntityType.ShardingDataSourceField);

            if (shardingKeyValue != null)
            {
                var routeWithValue = _virtualDataSourceRoute.RouteWithValue(_physicDataSources, shardingKeyValue);
                return new List<IPhysicDataSource>(1) {routeWithValue};
            }

            throw new NotImplementedException(nameof(VirutalDataSourceConfig));
        }

        public void AddDataSource(IPhysicDataSource physicTable)
        {
            if (_physicDataSources.Any(o => o.GetConnectionString() == physicTable.GetConnectionString()))
                return;
            _physicDataSources.Add(physicTable);
        }

        IVirtualDataSourceRoute<T> IVirtualDataSource<T>.GetRoute()
        {
            return _virtualDataSourceRoute;
        }

        public IVirtualDataSourceRoute GetRoute()
        {
            return _virtualDataSourceRoute;
        }
    }
}