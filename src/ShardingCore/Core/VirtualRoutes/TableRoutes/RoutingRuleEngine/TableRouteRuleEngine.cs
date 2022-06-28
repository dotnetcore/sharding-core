using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.ShardingDatabaseProviders;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;


namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:51:59
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleEngine : ITableRouteRuleEngine
    {
        private readonly ITableRouteManager _tableRouteManager;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IEntityMetadataManager _entityMetadataManager;
        private readonly IShardingDatabaseProvider _shardingDatabaseProvider;

        public TableRouteRuleEngine(ITableRouteManager tableRouteManager,IVirtualTableManager virtualTableManager,IEntityMetadataManager entityMetadataManager,IShardingDatabaseProvider shardingDatabaseProvider)
        {
            _tableRouteManager = tableRouteManager;
            _virtualTableManager = virtualTableManager;
            _entityMetadataManager = entityMetadataManager;
            _shardingDatabaseProvider = shardingDatabaseProvider;
        }

        public ShardingRouteUnit[] Route(TableRouteRuleContext tableRouteRuleContext)
        {
            Dictionary<Type, ISet<ShardingRouteUnit>> routeMaps = new Dictionary<Type, ISet<ShardingRouteUnit>>();
            var queryEntities = tableRouteRuleContext.QueryEntities;


            foreach (var shardingEntityKv in queryEntities)
            {
                var shardingEntity = shardingEntityKv.Key;
                if (!_entityMetadataManager.IsShardingTable(shardingEntity))
                    continue;
                var virtualTableRoute = _tableRouteManager.GetRoute(shardingEntity);
                var shardingRouteUnits = virtualTableRoute.RouteWithPredicate(tableRouteRuleContext.DataSourceRouteResult,(shardingEntityKv.Value ?? tableRouteRuleContext.Queryable),true);

                if (!routeMaps.ContainsKey(shardingEntity))
                {
                    routeMaps.Add(shardingEntity, shardingRouteUnits.ToHashSet());
                }
                else
                {
                    foreach (var shardingRouteUnit in shardingRouteUnits)
                    {
                        routeMaps[shardingEntity].Add(shardingRouteUnit);
                    }
                }
            }

            return routeMaps.Select(o => o.Value).Cartesian().Select(o => new TableRouteResult(o,_shardingDatabaseProvider.GetShardingDbContextType()));
        }
    }
}