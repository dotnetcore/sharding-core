using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Common;
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
        private readonly IEntityMetadataManager _entityMetadataManager;

        public TableRouteRuleEngine(ITableRouteManager tableRouteManager, 
            IEntityMetadataManager entityMetadataManager)
        {
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
        }

        private List<TableRouteUnit> GetEntityRouteUnit(DataSourceRouteResult dataSourceRouteResult,Type shardingEntity,IQueryable queryable)
        {
            if (!_entityMetadataManager.IsShardingTable(shardingEntity))
            {
                var dataSourceNames = dataSourceRouteResult.IntersectDataSources;
                var tableRouteUnits = new List<TableRouteUnit>(dataSourceNames.Count);
                foreach (var dataSourceName in dataSourceNames)
                {
                    var shardingRouteUnit = new TableRouteUnit(dataSourceName, string.Empty, shardingEntity);
                    tableRouteUnits.Add(shardingRouteUnit);
                }
                return tableRouteUnits;
            }
            var virtualTableRoute = _tableRouteManager.GetRoute(shardingEntity);
            return virtualTableRoute.RouteWithPredicate(dataSourceRouteResult, queryable, true);
        }
        public ShardingRouteResult Route(TableRouteRuleContext tableRouteRuleContext)
        {
            Dictionary<string /*dataSourceName*/, Dictionary<Type /*entityType*/, ISet<TableRouteUnit>>> routeMaps =
                new Dictionary<string, Dictionary<Type, ISet<TableRouteUnit>>>();
            var queryEntities = tableRouteRuleContext.QueryEntities;


            foreach (var shardingEntityKv in queryEntities)
            {
                var shardingEntity = shardingEntityKv.Key;
                var shardingRouteUnits = GetEntityRouteUnit(tableRouteRuleContext.DataSourceRouteResult,shardingEntity, shardingEntityKv.Value ?? tableRouteRuleContext.Queryable);
                
                foreach (var shardingRouteUnit in shardingRouteUnits)
                {
                    var dataSourceName = shardingRouteUnit.DataSourceName;


                    if (!routeMaps.ContainsKey(dataSourceName))
                    {
                        routeMaps.Add(dataSourceName,
                            new Dictionary<Type, ISet<TableRouteUnit>>()
                                { { shardingEntity, new HashSet<TableRouteUnit>() { shardingRouteUnit } } });
                    }
                    else
                    {
                        var routeMap = routeMaps[dataSourceName];
                        if (!routeMap.ContainsKey(shardingEntity))
                        {
                            routeMap.Add(shardingEntity, new HashSet<TableRouteUnit>() { shardingRouteUnit });
                        }
                        else
                        {
                            routeMap[shardingEntity].Add(shardingRouteUnit);
                        }
                    }
                }
            }

            //相同的数据源进行笛卡尔积
            //[[ds0,01,a],[ds0,02,a],[ds1,01,a]],[[ds0,01,b],[ds0,03,b],[ds1,01,b]]
            //=>
            //[ds0,[{01,a},{01,b}]],[ds0,[{01,a},{03,b}]],[ds0,[{02,a},{01,b}]],[ds0,[{02,a},{03,b}]],[ds1,[{01,a},{01,b}]]
            //如果笛卡尔积
            
            var sqlRouteUnits = new List<ISqlRouteUnit>(31);
            int dataSourceCount = 0;
            bool isCrossTable = false;
            bool existCrossTableTails = false;
            foreach (var dataSourceName in tableRouteRuleContext.DataSourceRouteResult.IntersectDataSources)
            {
                if (routeMaps.ContainsKey(dataSourceName))
                {
                    var routeMap = routeMaps[dataSourceName];
                    var tableRouteResults = routeMap.Select(o => o.Value).Cartesian()
                        .Select(o => new TableRouteResult(o.ToList())).Where(o=>!o.IsEmpty).ToList();
                    if (tableRouteResults.IsNotEmpty())
                    {
                        dataSourceCount++;
                        if (tableRouteResults.Count > 1)
                        {
                            isCrossTable = true;
                        }
                        foreach (var tableRouteResult in tableRouteResults)
                        {
                            if (tableRouteResult.ReplaceTables.Count > 1)
                            {
                                isCrossTable = true;
                                if (tableRouteResult.HasDifferentTail)
                                {
                                    existCrossTableTails = true;
                                }
                            }
                            sqlRouteUnits.Add(new SqlRouteUnit(dataSourceName, tableRouteResult));
                        }
                    }
                }
            }

           return new ShardingRouteResult(sqlRouteUnits, sqlRouteUnits.Count == 0, dataSourceCount > 1, isCrossTable,
                existCrossTableTails);
            //
            // var sqlRouteUnits = tableRouteRuleContext.DataSourceRouteResult.IntersectDataSources.SelectMany(
            //     dataSourceName =>
            //     {
            //         return routeMaps.Select(o => o.Value.Where(route => route.DataSourceName == dataSourceName))
            //             .Cartesian()
            //             .Select(o => (ISqlRouteUnit)new SqlRouteUnit(dataSourceName, new TableRouteResult(o.ToList())))
            //             .ToArray();
            //     }).Where(o => o.TableRouteResult.ReplaceTables.Any()).ToArray();
            // return sqlRouteUnits;
            // return routeMaps.Select(o => o.Value).Cartesian().Where(o=>o).Select(o => new TableRouteResult(o,_shardingDatabaseProvider.GetShardingDbContextType()));
        }
        
    }
}