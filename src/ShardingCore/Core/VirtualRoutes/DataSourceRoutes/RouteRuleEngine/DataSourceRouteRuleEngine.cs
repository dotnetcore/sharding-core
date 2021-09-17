using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 13:00:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DataSourceRouteRuleEngine : IDataSourceRouteRuleEngine
    {
        private readonly IVirtualDataSourceManager _virtualDataSourceManager;

        public DataSourceRouteRuleEngine(IVirtualDataSourceManager virtualDataSourceManager)
        {
            _virtualDataSourceManager = virtualDataSourceManager;
        }

        public DataSourceRouteResult Route<T, TShardingDbContext>(DataSourceRouteRuleContext<T> routeRuleContext) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return this.Route<T>(typeof(TShardingDbContext), routeRuleContext);
        }

        public DataSourceRouteResult Route<T>(Type shardingDbContextType, DataSourceRouteRuleContext<T> routeRuleContext)
        {
            if (!shardingDbContextType.IsShardingDataSource())
                throw new InvalidOperationException($"{shardingDbContextType} must impl {nameof(IShardingDbContext)}");
            var dataSourceMaps = new Dictionary<Type, ISet<IPhysicDataSource>>();
            var notShardingDataSourceEntityType = routeRuleContext.QueryEntities.FirstOrDefault(o => !o.IsShardingDataSource());
            //存在不分表的
            if (notShardingDataSourceEntityType != null)
                dataSourceMaps.Add(notShardingDataSourceEntityType, new HashSet<IPhysicDataSource>() { _virtualDataSourceManager.GetDefaultDataSource(shardingDbContextType) });


            var queryEntities = routeRuleContext.QueryEntities.Where(o => o.IsShardingDataSource()).ToList();
            if (queryEntities.Count > 1)
                throw new ShardingCoreNotSupportedException($"{routeRuleContext.Queryable.ShardingPrint()}");
            foreach (var queryEntity in queryEntities)
            {
                var virtualDataSource = _virtualDataSourceManager.GetVirtualDataSource(queryEntity);
                var dataSourceConfigs = virtualDataSource.RouteTo(new ShardingDataSourceRouteConfig(routeRuleContext.Queryable));
                if (!dataSourceMaps.ContainsKey(queryEntity))
                {
                    dataSourceMaps.Add(queryEntity, dataSourceConfigs.ToHashSet());
                }
                else
                {
                    foreach (var shardingDataSource in dataSourceConfigs)
                    {
                        dataSourceMaps[queryEntity].Add(shardingDataSource);
                    }
                }
            }

            if (dataSourceMaps.IsEmpty())
                throw new ShardingDataSourceRouteNotMatchException(
                    $"{routeRuleContext.Queryable.ShardingPrint()}");
            if (dataSourceMaps.Count == 1)
                return new DataSourceRouteResult(dataSourceMaps.First().Value);
            var intersect = dataSourceMaps.Select(o => o.Value).Aggregate((p, n) => p.Intersect(n).ToHashSet());
            return new DataSourceRouteResult(intersect);
        }
    }
}
