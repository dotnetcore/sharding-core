using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ParallelTables;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class MergeQueryCompilerContext : IMergeQueryCompilerContext
    {

        private readonly IParallelTableManager _parallelTableManager;
        private readonly IQueryCompilerContext _queryCompilerContext;
        private readonly QueryCombineResult _queryCombineResult;
        private readonly DataSourceRouteResult _dataSourceRouteResult;
        private readonly TableRouteResult[] _tableRouteResults;

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
        private MergeQueryCompilerContext(IQueryCompilerContext queryCompilerContext, QueryCombineResult queryCombineResult, DataSourceRouteResult dataSourceRouteResult, IEnumerable<TableRouteResult> tableRouteResults)
        {
            _queryCompilerContext = queryCompilerContext;
            _queryCombineResult = queryCombineResult;
            _parallelTableManager = (IParallelTableManager)ShardingContainer.GetService(typeof(IParallelTableManager<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            _dataSourceRouteResult = dataSourceRouteResult;
            _tableRouteResults = GetTableRouteResults(tableRouteResults).ToArray();
            _isCrossDataSource = dataSourceRouteResult.IntersectDataSources.Count > 1;
            _isCrossTable = _tableRouteResults.Count() > 1;
            _existCrossTableTails = _tableRouteResults.Any(o => o.HasDifferentTail);
        }

        private IEnumerable<TableRouteResult> GetTableRouteResults(IEnumerable<TableRouteResult> tableRouteResults)
        {
            var routeResults = tableRouteResults as TableRouteResult[] ?? tableRouteResults.ToArray();
            if (_queryCompilerContext.GetQueryEntities().Count > 1&& routeResults.Length>0)
            {
                var entityMetadataManager = _queryCompilerContext.GetEntityMetadataManager();
                var queryShardingTables = _queryCompilerContext.GetQueryEntities().Keys.Where(o => entityMetadataManager.IsShardingTable(o)).ToArray();
                if (queryShardingTables.Length > 1 && _parallelTableManager.IsParallelTableQuery(queryShardingTables))
                {
                    return routeResults.Where(o => o.ReplaceTables.Select(p => p.Tail).ToHashSet().Count == 1);
                }
            }
            return routeResults;
        }

        public static MergeQueryCompilerContext Create(IQueryCompilerContext queryCompilerContext, QueryCombineResult queryCombineResult, DataSourceRouteResult dataSourceRouteResult,IEnumerable<TableRouteResult> tableRouteResults)
        {
            return new MergeQueryCompilerContext(queryCompilerContext, queryCombineResult,dataSourceRouteResult, tableRouteResults);
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
            if (!hasQueryCompilerExecutor.HasValue)
            {
                if (_dataSourceRouteResult.IntersectDataSources.IsEmpty() || _tableRouteResults.IsEmpty())
                {
                    hasQueryCompilerExecutor = false;
                }
                else
                {
                    hasQueryCompilerExecutor = IsSingleQuery();
                    if (hasQueryCompilerExecutor.Value)
                    {
                        //要么本次查询不追踪如果需要追踪不可以存在跨tails
                        var routeTailFactory = ShardingContainer.GetService<IRouteTailFactory>();
                        var dbContext = GetShardingDbContext().GetDbContext(_dataSourceRouteResult.IntersectDataSources.First(), IsParallelQuery(), routeTailFactory.Create(_tableRouteResults[0]));
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

        public TableRouteResult[] GetTableRouteResults()
        {
            return _tableRouteResults;
        }

        public DataSourceRouteResult GetDataSourceRouteResult()
        {
            return _dataSourceRouteResult;
        }
        /// <summary>
        /// 既不可以跨库也不可以跨表,所有的分表都必须是相同后缀才可以
        /// </summary>
        /// <returns></returns>
        private bool IsSingleQuery()
        {
            return !_isCrossDataSource&&!_isCrossTable;
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

        /// <summary>
        /// 如果需要聚合并且存在跨tail的查询或者本次是读链接
        /// </summary>
        /// <returns></returns>
        public bool IsParallelQuery()
        {
            return _isCrossTable || _existCrossTableTails|| _queryCompilerContext.IsParallelQuery();
        }
    }
}
