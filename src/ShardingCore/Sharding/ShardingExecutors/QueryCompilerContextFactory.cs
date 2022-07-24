using Microsoft.Extensions.Logging;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;
using System;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Extensions.InternalExtensions;

using ShardingCore.Sharding.Parsers.Abstractions;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContextFactory : IQueryCompilerContextFactory
    {
        private readonly IDataSourceRouteRuleEngineFactory _dataSourceRouteRuleEngineFactory;
        private readonly ITableRouteRuleEngineFactory _tableRouteRuleEngineFactory;

        private  readonly ILogger<QueryCompilerContextFactory> _logger;
        private  readonly bool _logDebug;
        private static readonly IQueryableCombine _enumerableQueryableCombine;
        private static readonly IQueryableCombine _allQueryableCombine;
        private static readonly IQueryableCombine _constantQueryableCombine;
        private static readonly IQueryableCombine _selectQueryableCombine;
        private static readonly IQueryableCombine _whereQueryableCombine;

        static QueryCompilerContextFactory()
        {
            _enumerableQueryableCombine = new EnumerableQueryableCombine();
            _allQueryableCombine = new AllQueryableCombine();
            _constantQueryableCombine = new ConstantQueryableCombine();
            _selectQueryableCombine = new SelectQueryableCombine();
            _whereQueryableCombine = new WhereQueryableCombine();
        }

        public QueryCompilerContextFactory(IDataSourceRouteRuleEngineFactory dataSourceRouteRuleEngineFactory,ITableRouteRuleEngineFactory tableRouteRuleEngineFactory,ILogger<QueryCompilerContextFactory> logger)
        {
            _dataSourceRouteRuleEngineFactory = dataSourceRouteRuleEngineFactory;
            _tableRouteRuleEngineFactory = tableRouteRuleEngineFactory;
           _logger = logger;
           _logDebug = _logger.IsEnabled(LogLevel.Debug);
        }

        public IQueryCompilerContext Create(IPrepareParseResult prepareParseResult)
        {
            var queryCompilerContext =
                QueryCompilerContext.Create(prepareParseResult);
            if (queryCompilerContext.GetQueryCompilerExecutor() is not null)
            {
                if (_logDebug)
                {
                    _logger.LogDebug($"{queryCompilerContext.GetQueryExpression().ShardingPrint()} is native query");
                }
                return queryCompilerContext;
            }

            var queryableCombine = GetQueryableCombine(queryCompilerContext);
            if (_logDebug)
            {
                _logger.LogDebug($"queryable combine:{queryableCombine.GetType()}");
                _logger.LogDebug($"queryable combine before:{queryCompilerContext.GetQueryExpression().ShardingPrint()}");
            }
            var queryCombineResult = queryableCombine.Combine(queryCompilerContext);
            if (_logDebug)
            {
                _logger.LogDebug($"queryable combine after:{queryCombineResult.GetCombineQueryable().ShardingPrint()}");
            }
            var dataSourceRouteResult = _dataSourceRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable(), prepareParseResult.GetShardingDbContext(), prepareParseResult.GetQueryEntities());
            if (_logDebug)
            {
                _logger.LogDebug($"{dataSourceRouteResult}");
            }
            var shardingRouteResult = _tableRouteRuleEngineFactory.Route(dataSourceRouteResult,queryCombineResult.GetCombineQueryable(), prepareParseResult.GetQueryEntities());
            if (_logDebug)
            {
                _logger.LogDebug($"table route results:{shardingRouteResult}");
            }
            var mergeCombineCompilerContext = MergeQueryCompilerContext.Create(queryCompilerContext, queryCombineResult, shardingRouteResult);
            return mergeCombineCompilerContext;
        }

        private IQueryableCombine GetQueryableCombine(IQueryCompilerContext queryCompilerContext)
        {
            if (queryCompilerContext.IsEnumerableQuery())
            {
                return _enumerableQueryableCombine;
            }
            else
            {
                return GetMethodQueryableCombine(queryCompilerContext);
            }
        }

        private IQueryableCombine GetMethodQueryableCombine(IQueryCompilerContext queryCompilerContext)
        {
            string methodName=null;
            if (queryCompilerContext.GetQueryExpression() is MethodCallExpression methodCallExpression)
            {
                methodName = methodCallExpression.Method.Name;
                switch (methodName)
                {
                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                    case nameof(Queryable.Last):
                    case nameof(Queryable.LastOrDefault):
                    case nameof(Queryable.Single):
                    case nameof(Queryable.SingleOrDefault):
                    case nameof(Queryable.Count):
                    case nameof(Queryable.LongCount):
                    case nameof(Queryable.Any):
                        return _whereQueryableCombine;
                    case nameof(Queryable.All):
                        return _allQueryableCombine;
                    case nameof(Queryable.Max):
                    case nameof(Queryable.Min):
                    case nameof(Queryable.Sum):
                    case nameof(Queryable.Average):
                        return _selectQueryableCombine;
                    case nameof(Queryable.Contains):
                        return _constantQueryableCombine;
                }
            }

            throw new ShardingCoreException($"query expression:[{methodName}] is not terminate operate");
        }
    }
}