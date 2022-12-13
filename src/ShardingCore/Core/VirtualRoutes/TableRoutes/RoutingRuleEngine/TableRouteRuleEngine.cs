using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.ParallelTables;


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
        private readonly IParallelTableManager _parallelTableManager;

        public TableRouteRuleEngine(ITableRouteManager tableRouteManager,
            IEntityMetadataManager entityMetadataManager,IParallelTableManager parallelTableManager)
        {
            _tableRouteManager = tableRouteManager;
            _entityMetadataManager = entityMetadataManager;
            _parallelTableManager = parallelTableManager;
        }

        private List<TableRouteUnit> GetEntityRouteUnit(DataSourceRouteResult dataSourceRouteResult,
            Type shardingEntity, IQueryable queryable)
        {
            // if (!_entityMetadataManager.IsShardingTable(shardingEntity))
            // {
            //     var dataSourceNames = dataSourceRouteResult.IntersectDataSources;
            //     var tableRouteUnits = new List<TableRouteUnit>(dataSourceNames.Count);
            //     foreach (var dataSourceName in dataSourceNames)
            //     {
            //         var shardingRouteUnit = new TableRouteUnit(dataSourceName, string.Empty, shardingEntity);
            //         tableRouteUnits.Add(shardingRouteUnit);
            //     }
            //     return tableRouteUnits;
            // }
            var virtualTableRoute = _tableRouteManager.GetRoute(shardingEntity);
            return virtualTableRoute.RouteWithPredicate(dataSourceRouteResult, queryable, true);
        }

        public ShardingRouteResult Route(TableRouteRuleContext tableRouteRuleContext)
        {
            Dictionary<string /*dataSourceName*/, Dictionary<Type /*entityType*/, ISet<TableRouteUnit>>> routeMaps =
                new Dictionary<string, Dictionary<Type, ISet<TableRouteUnit>>>();
            var queryEntities = tableRouteRuleContext.QueryEntities;


            bool onlyShardingDataSource = queryEntities.All(o=>_entityMetadataManager.IsOnlyShardingDataSource(o.Key));
            foreach (var shardingEntityKv in queryEntities)
            {
                var shardingEntity = shardingEntityKv.Key;
                if (!_entityMetadataManager.IsShardingTable(shardingEntity))
                {
                    continue;
                }


                var shardingRouteUnits = GetEntityRouteUnit(tableRouteRuleContext.DataSourceRouteResult, shardingEntity,
                    shardingEntityKv.Value ?? tableRouteRuleContext.Queryable);

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
                    var routeResults = routeMap.Select(o => o.Value).Cartesian()
                        .Select(o => new TableRouteResult(o.ToList())).Where(o => !o.IsEmpty).ToArray();

                    var tableRouteResults = GetTableRouteResults(tableRouteRuleContext, routeResults);
                    if (tableRouteResults.IsNotEmpty())
                    {
                        dataSourceCount++;
                        if (tableRouteResults.Length > 1)
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
                }else if (onlyShardingDataSource)
                {
                    var tableRouteResult = new TableRouteResult(queryEntities.Keys.Select(o=>new TableRouteUnit(dataSourceName,String.Empty,o )).ToList());
                    sqlRouteUnits.Add(new SqlRouteUnit(dataSourceName, tableRouteResult));
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
        private TableRouteResult[] GetTableRouteResults(TableRouteRuleContext tableRouteRuleContext,TableRouteResult[] routeResults)
        {
            if (tableRouteRuleContext.QueryEntities.Count > 1&& routeResults.Length>0)
            {
                var queryShardingTables = tableRouteRuleContext.QueryEntities.Keys.Where(o => _entityMetadataManager.IsShardingTable(o)).ToArray();
                if (queryShardingTables.Length > 1 && _parallelTableManager.IsParallelTableQuery(queryShardingTables))
                {
                    return routeResults.Where(o => !o.HasDifferentTail).ToArray();
                }
            }
            return routeResults;
        }
    }
    
}