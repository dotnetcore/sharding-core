using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class MergeQueryCompilerContext : IMergeQueryCompilerContext
    {

        private readonly IParallelTableManager _parallelTableManager;
        private readonly IShardingRuntimeContext _shardingRuntimeContext;
        private readonly IQueryCompilerContext _queryCompilerContext;
        private readonly QueryCombineResult _queryCombineResult;
        private readonly ShardingRouteResult _shardingRouteResult;

        /// <summary>
        /// 本次查询跨库
        /// </summary>
        private readonly bool _isCrossDataSource;

        /// <summary>
        /// 本次查询跨表
        /// </summary>
        private readonly bool _isCrossTable;
        /// <summary>
        /// 存在一次查询跨多个tail
        /// </summary>
        private readonly bool _existCrossTableTails;


        private QueryCompilerExecutor _queryCompilerExecutor;
        private bool? hasQueryCompilerExecutor;
        private readonly int? _fixedTake;
        private MergeQueryCompilerContext(IShardingRuntimeContext shardingRuntimeContext,IQueryCompilerContext queryCompilerContext, QueryCombineResult queryCombineResult,  ShardingRouteResult shardingRouteResult)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
            _queryCompilerContext = queryCompilerContext;
            _queryCombineResult = queryCombineResult;
            _shardingRouteResult = shardingRouteResult;
            _parallelTableManager = _shardingRuntimeContext.GetParallelTableManager();
            // _tableRouteResults = GetTableRouteResults(sqlRouteUnits).ToArray();
            _isCrossDataSource = shardingRouteResult.IsCrossDataSource;
            _isCrossTable = shardingRouteResult.IsCrossTable;
            _existCrossTableTails = shardingRouteResult.ExistCrossTableTails;
            var queryMethodName = queryCompilerContext.GetQueryMethodName();
            _fixedTake = GetMethodNameFixedTake(queryMethodName);
        }

        private int? GetMethodNameFixedTake(string queryMethodName)
        {
            switch (queryMethodName)
            {
                case nameof(Enumerable.First):
                case nameof(Enumerable.FirstOrDefault):
                    return 1;
                case nameof(Enumerable.Single):
                case nameof(Enumerable.SingleOrDefault):
                    return 2;
                case nameof(Enumerable.Last):
                case nameof(Enumerable.LastOrDefault):
                    return 1;
            }

            return null;
        }
        //
        // private IEnumerable<TableRouteResult> GetTableRouteResults(IEnumerable<TableRouteResult> tableRouteResults)
        // {
        //     var routeResults = tableRouteResults as TableRouteResult[] ?? tableRouteResults.ToArray();
        //     if (_queryCompilerContext.GetQueryEntities().Count > 1&& routeResults.Length>0)
        //     {
        //         var entityMetadataManager = _queryCompilerContext.GetEntityMetadataManager();
        //         var queryShardingTables = _queryCompilerContext.GetQueryEntities().Keys.Where(o => entityMetadataManager.IsShardingTable(o)).ToArray();
        //         if (queryShardingTables.Length > 1 && _parallelTableManager.IsParallelTableQuery(queryShardingTables))
        //         {
        //             return routeResults.Where(o => o.ReplaceTables.Select(p => p.Tail).ToHashSet().Count == 1);
        //         }
        //     }
        //     return routeResults;
        // }

        public static MergeQueryCompilerContext Create(IQueryCompilerContext queryCompilerContext, QueryCombineResult queryCombineResult, ShardingRouteResult shardingRouteResult)
        {
            var shardingDbContext = queryCompilerContext.GetShardingDbContext();
            var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
            return new MergeQueryCompilerContext(shardingRuntimeContext,queryCompilerContext, queryCombineResult,shardingRouteResult);
        }
        public Dictionary<Type,IQueryable> GetQueryEntities()
        {
            return _queryCompilerContext.GetQueryEntities();
        }

        public IShardingDbContext GetShardingDbContext()
        {
            return _queryCompilerContext.GetShardingDbContext();
        }

        public Expression GetQueryExpression()
        {
            return _queryCompilerContext.GetQueryExpression();
        }

        public IEntityMetadataManager GetEntityMetadataManager()
        {
            return _queryCompilerContext.GetEntityMetadataManager();
        }

        public Type GetShardingDbContextType()
        {
            return _queryCompilerContext.GetShardingDbContextType();
        }

        public bool IsQueryTrack()
        {
            return _queryCompilerContext.IsQueryTrack();
        }

        public bool UseUnionAllMerge()
        {
            return _queryCompilerContext.UseUnionAllMerge();
        }

        public int? GetMaxQueryConnectionsLimit()
        {
            return _queryCompilerContext.GetMaxQueryConnectionsLimit();
        }

        public ConnectionModeEnum? GetConnectionMode()
        {
            return _queryCompilerContext.GetConnectionMode();
        }

        public bool? IsSequence()
        {
            return _queryCompilerContext.IsSequence();
        }

        public bool? SameWithShardingComparer()
        {
            return _queryCompilerContext.SameWithShardingComparer();
        }

        public bool IsSingleShardingEntityQuery()
        {
            return _queryCompilerContext.IsSingleShardingEntityQuery();
        }

        public Type GetSingleShardingEntityType()
        {
            return _queryCompilerContext.GetSingleShardingEntityType();
        }

        public QueryCompilerExecutor GetQueryCompilerExecutor()
        {
            //只获取第一次所以判断是否已经获取过了
            if (!hasQueryCompilerExecutor.HasValue)
            {
                //空结果
                //todo 后续优化直接无需后续的解析之类的
                if(_shardingRouteResult.IsEmpty)
                {
                    hasQueryCompilerExecutor = false;
                }
                else
                {
                    hasQueryCompilerExecutor = IsSingleQuery();
                    if (hasQueryCompilerExecutor.Value)
                    {
                        //要么本次查询不追踪如果需要追踪不可以存在跨tails
                        var routeTailFactory = _shardingRuntimeContext.GetRouteTailFactory();
                        var sqlRouteUnit = _shardingRouteResult.RouteUnits.First();
                        var strategy = !IsParallelQuery()
                            ? CreateDbContextStrategyEnum.ShareConnection
                            : CreateDbContextStrategyEnum.IndependentConnectionQuery;
                        var dbContext = GetShardingDbContext().GetDbContext(sqlRouteUnit.DataSourceName,strategy , routeTailFactory.Create(sqlRouteUnit.TableRouteResult));
                        _queryCompilerExecutor = new QueryCompilerExecutor(dbContext, GetQueryExpression());
                    }
                }
            }

            return _queryCompilerExecutor;
        }


        public QueryCombineResult GetQueryCombineResult()
        {
            return _queryCombineResult;
        }

        public ShardingRouteResult GetShardingRouteResult()
        {
            return _shardingRouteResult;
        }

        /// <summary>
        /// 既不可以跨库也不可以跨表,所有的分表都必须是相同后缀才可以
        /// </summary>
        /// <returns></returns>
        private bool IsSingleQuery()
        {
            return _shardingRouteResult.RouteUnits.Count==1;//&& !_isCrossDataSource&&!_isCrossTable;
        }

        public bool IsCrossTable()
        {
            return _isCrossTable;
        }

        public bool IsCrossDataSource()
        {
            return _isCrossDataSource;
        }

        public bool IsEnumerableQuery()
        {
            return _queryCompilerContext.IsEnumerableQuery();
        }

        public string GetQueryMethodName()
        {
            return _queryCompilerContext.GetQueryMethodName();
        }

        /// <summary>
        /// 如果需要聚合并且存在跨tail的查询或者本次是读链接
        /// </summary>
        /// <returns></returns>
        public bool IsParallelQuery()
        {
            return _isCrossTable || _existCrossTableTails|| _queryCompilerContext.IsParallelQuery();
        }

        public int? GetFixedTake()
        {
            return _fixedTake;
        }
    }
}
