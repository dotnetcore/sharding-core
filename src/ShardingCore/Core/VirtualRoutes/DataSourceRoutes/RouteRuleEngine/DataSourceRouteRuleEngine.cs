using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Core.VirtualRoutes.Abstractions;
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
    public class DataSourceRouteRuleEngine: IDataSourceRouteRuleEngine
    {
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IVirtualDataSource _virtualDataSource;
        private readonly IDataSourceRouteManager _dataSourceRouteManager;

        public DataSourceRouteRuleEngine(IEntityMetadataManager entityMetadataManager,IVirtualDataSource virtualDataSource,IDataSourceRouteManager dataSourceRouteManager)
        {
            _entityMetadataManager = entityMetadataManager;
            _virtualDataSource = virtualDataSource;
            _dataSourceRouteManager = dataSourceRouteManager;
        }
        public DataSourceRouteResult Route(DataSourceRouteRuleContext routeRuleContext)
        {
            var dataSourceMaps = new Dictionary<Type, ISet<string>>();

            foreach (var queryEntityKv in routeRuleContext.QueryEntities)
            {
                var queryEntity = queryEntityKv.Key;
                if (!_entityMetadataManager.IsShardingDataSource(queryEntity))
                {
                    dataSourceMaps.Add(queryEntity, new HashSet<string>() { _virtualDataSource.DefaultDataSourceName });
                    continue;
                }
                var dataSourceConfigs = _dataSourceRouteManager.RouteTo(queryEntity, new ShardingDataSourceRouteConfig(queryEntityKv.Value??routeRuleContext.Queryable));
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
                throw new ShardingCoreException(
                    $"data source route not match: {routeRuleContext.Queryable.ShardingPrint()}");
            if (dataSourceMaps.Count == 1)
                return new DataSourceRouteResult(dataSourceMaps.First().Value);
            var intersect = dataSourceMaps.Select(o => o.Value).Aggregate((p, n) => p.Intersect(n).ToHashSet());
            return new DataSourceRouteResult(intersect);
        }
    }
}
