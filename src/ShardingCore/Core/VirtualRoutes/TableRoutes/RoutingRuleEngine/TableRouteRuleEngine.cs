using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;


namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:51:59
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleEngine<TShardingDbContext> : ITableRouteRuleEngine<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IVirtualTableManager<TShardingDbContext> _virtualTableManager;
        private readonly IEntityMetadataManager<TShardingDbContext> _entityMetadataManager;

        public TableRouteRuleEngine(IVirtualTableManager<TShardingDbContext> virtualTableManager,IEntityMetadataManager<TShardingDbContext> entityMetadataManager)
        {
            _virtualTableManager = virtualTableManager;
            _entityMetadataManager = entityMetadataManager;
        }

        public IEnumerable<TableRouteResult> Route(TableRouteRuleContext tableRouteRuleContext)
        {
            Dictionary<IVirtualTable, ISet<IPhysicTable>> routeMaps = new Dictionary<IVirtualTable, ISet<IPhysicTable>>();
            var queryEntities = tableRouteRuleContext.QueryEntities;


            foreach (var shardingEntityKv in queryEntities)
            {
                var shardingEntity = shardingEntityKv.Key;
                if (!_entityMetadataManager.IsShardingTable(shardingEntity))
                    continue;
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntity);

                var physicTables = virtualTable.RouteTo(new ShardingTableRouteConfig(shardingEntityKv.Value ?? tableRouteRuleContext.Queryable));
                if (!routeMaps.ContainsKey(virtualTable))
                {
                    routeMaps.Add(virtualTable, physicTables.ToHashSet());
                }
                else
                {
                    foreach (var physicTable in physicTables)
                    {
                        routeMaps[virtualTable].Add(physicTable);
                    }
                }
            }

            return routeMaps.Select(o => o.Value).Cartesian().Select(o => new TableRouteResult(o,typeof(TShardingDbContext)));
        }
    }
}