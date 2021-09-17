using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

#if !EFCORE5
using ShardingCore.Extensions;
#endif

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:51:59
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleEngines:ITableRouteRuleEngine
    {
        private readonly IVirtualTableManager _virtualTableManager;

        public TableRouteRuleEngines(IVirtualTableManager virtualTableManager)
        {
            _virtualTableManager = virtualTableManager;
        }

        public IEnumerable<TableRouteResult> Route<T, TShardingDbContext>(TableRouteRuleContext<T> tableRouteRuleContext) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return Route(typeof(TShardingDbContext), tableRouteRuleContext);
            ////先添加手动路由到当前上下文,之后将不再手动路由里面的自动路由添加到当前上下文
            //foreach (var kv in tableRouteRuleContext.ManualTails)
            //{
            //    var virtualTable = kv.Key;
            //    var addTails = kv.Value;
            //    var physicTables = virtualTable.GetAllPhysicTables().Where(o=>addTails.Contains(o.Tail));
            //    if (!routeMaps.ContainsKey(virtualTable))
            //    {
            //        routeMaps.Add(virtualTable,physicTables.ToHashSet());
            //    }
            //    else
            //    {
            //        foreach (var physicTable in physicTables)
            //        {
            //            routeMaps[virtualTable].Add(physicTable);
            //        }
            //    }
            //}  
            //foreach (var kv in tableRouteRuleContext.ManualPredicate)
            //{
            //    var virtualTable = kv.Key;
            //    var predicate = kv.Value;
            //    var physicTables = virtualTable.RouteTo(new TableRouteConfig(null, null, null, predicate));
            //    if (!routeMaps.ContainsKey(virtualTable))
            //    {
            //        routeMaps.Add(virtualTable, physicTables.ToHashSet());
            //    }
            //    else
            //    {
            //        foreach (var physicTable in physicTables)
            //        {
            //            routeMaps[virtualTable].Add(physicTable);
            //        }
            //    }
            //}

            //if (tableRouteRuleContext.AutoParseRoute)
            //{
            //    var shardingEntities = queryEntities.Where(o => o.IsShardingTable());
            //    foreach (var shardingEntity in shardingEntities)
            //    {
            //        var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntity);

            //        var physicTables = virtualTable.RouteTo(new TableRouteConfig(tableRouteRuleContext.Queryable));
            //        if (!routeMaps.ContainsKey(virtualTable))
            //        {
            //            routeMaps.Add(virtualTable, physicTables.ToHashSet());
            //        }
            //        else
            //        {
            //            foreach (var physicTable in physicTables)
            //            {
            //                routeMaps[virtualTable].Add(physicTable);
            //            }
            //        }
            //    }
            //}
        }

        public IEnumerable<TableRouteResult> Route<T>(Type shardingDbContextType, TableRouteRuleContext<T> tableRouteRuleContext)
        {
            Dictionary<IVirtualTable, ISet<IPhysicTable>> routeMaps = new Dictionary<IVirtualTable, ISet<IPhysicTable>>();
            var queryEntities = tableRouteRuleContext.Queryable.ParseQueryableRoute();


            var shardingEntities = queryEntities.Where(o => o.IsShardingTable());
            foreach (var shardingEntity in shardingEntities)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingDbContextType, shardingEntity);

                var physicTables = virtualTable.RouteTo(new ShardingTableRouteConfig(tableRouteRuleContext.Queryable));
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

            return routeMaps.Select(o => o.Value).Cartesian().Select(o => new TableRouteResult(o));
        }
    }
}