using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Extensions;
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
    public class QueryRouteRuleEngines:IRouteRuleEngine
    {
        private readonly IVirtualTableManager _virtualTableManager;

        public QueryRouteRuleEngines(IVirtualTableManager virtualTableManager)
        {
            _virtualTableManager = virtualTableManager;
        }

        public IEnumerable<RouteResult> Route<T>(RouteRuleContext<T> routeRuleContext)
        {
            Dictionary<IVirtualTable, ISet<IPhysicTable>> routeMaps = new Dictionary<IVirtualTable, ISet<IPhysicTable>>();
            var queryEntities = routeRuleContext.Queryable.ParseQueryableRoute();


            var shardingEntities = queryEntities.Where(o => o.IsShardingTable());
            foreach (var shardingEntity in shardingEntities)
            {
                var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntity);

                var physicTables = virtualTable.RouteTo(new TableRouteConfig(routeRuleContext.Queryable));
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
            ////先添加手动路由到当前上下文,之后将不再手动路由里面的自动路由添加到当前上下文
            //foreach (var kv in routeRuleContext.ManualTails)
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
            //foreach (var kv in routeRuleContext.ManualPredicate)
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

            //if (routeRuleContext.AutoParseRoute)
            //{
            //    var shardingEntities = queryEntities.Where(o => o.IsShardingTable());
            //    foreach (var shardingEntity in shardingEntities)
            //    {
            //        var virtualTable = _virtualTableManager.GetVirtualTable(shardingEntity);

            //        var physicTables = virtualTable.RouteTo(new TableRouteConfig(routeRuleContext.Queryable));
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

            return routeMaps.Select(o => o.Value).Cartesian().Select(o=>new RouteResult(o));
        }
    }
}