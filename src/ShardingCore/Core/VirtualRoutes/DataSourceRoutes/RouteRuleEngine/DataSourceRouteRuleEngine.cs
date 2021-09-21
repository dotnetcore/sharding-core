using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
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
    public class DataSourceRouteRuleEngine<TShardingDbContext> : IDataSourceRouteRuleEngine<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IVirtualDataSource<TShardingDbContext> _virtualDataSource;

        public DataSourceRouteRuleEngine(IVirtualDataSource<TShardingDbContext> virtualDataSource)
        {
            _virtualDataSource = virtualDataSource;
        }
        public DataSourceRouteResult Route<T>(DataSourceRouteRuleContext<T> routeRuleContext)
        {
            var dataSourceMaps = new Dictionary<Type, ISet<string>>();
            var notShardingDataSourceEntityType = routeRuleContext.QueryEntities.FirstOrDefault(o => !o.IsShardingDataSource());
            //存在不分库的
            if (notShardingDataSourceEntityType != null)
                dataSourceMaps.Add(notShardingDataSourceEntityType, new HashSet<string>() { _virtualDataSource.DefaultDataSourceName });


            var queryEntities = routeRuleContext.QueryEntities.Where(o => o.IsShardingDataSource()).ToList();
            if (queryEntities.Count > 1)
                throw new ShardingCoreNotSupportedException($"{routeRuleContext.Queryable.ShardingPrint()}");
            foreach (var queryEntity in queryEntities)
            {
                var dataSourceConfigs = _virtualDataSource.RouteTo(queryEntity,new ShardingDataSourceRouteConfig(routeRuleContext.Queryable));
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
