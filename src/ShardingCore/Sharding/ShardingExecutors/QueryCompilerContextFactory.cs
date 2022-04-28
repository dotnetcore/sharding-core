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

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContextFactory : IQueryCompilerContextFactory
    {
        private readonly ILogger<QueryCompilerContextFactory> _logger;
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

        public QueryCompilerContextFactory(ILogger<QueryCompilerContextFactory> logger)
        {
            _logger = logger;
        }

        public IQueryCompilerContext Create(ICompileParameter compileParameter)
        {
            var queryCompilerContext =
                QueryCompilerContext.Create(compileParameter);
            if (queryCompilerContext.GetQueryCompilerExecutor() is not null)
            {
                _logger.LogDebug($"{queryCompilerContext.GetQueryExpression().ShardingPrint()} is native query");
                return queryCompilerContext;
            }

            var queryableCombine = GetQueryableCombine(queryCompilerContext);
            _logger.LogDebug($"queryable combine:{queryableCombine.GetType()}");
            var dataSourceRouteRuleEngineFactory = (IDataSourceRouteRuleEngineFactory)ShardingContainer.GetService(typeof(IDataSourceRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            var tableRouteRuleEngineFactory = (ITableRouteRuleEngineFactory)ShardingContainer.GetService(typeof(ITableRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            _logger.LogDebug($"queryable combine before:{queryCompilerContext.GetQueryExpression().ShardingPrint()}");
            var queryCombineResult = queryableCombine.Combine(queryCompilerContext);
            _logger.LogDebug($"queryable combine after:{queryCombineResult.GetCombineQueryable().ShardingPrint()}");
            var dataSourceRouteResult = dataSourceRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable(), compileParameter.GetShardingDbContext());
            _logger.LogDebug(dataSourceRouteResult.GetPrintInfo());
            var routeResults = tableRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable()).ToArray();
            _logger.LogDebug($"table route results:{string.Join(","+Environment.NewLine,routeResults.Select(o=>o.GetPrintInfo()))}");
            var mergeCombineCompilerContext = MergeQueryCompilerContext.Create(queryCompilerContext, queryCombineResult, dataSourceRouteResult,
                routeResults);
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
            if (queryCompilerContext.GetQueryExpression() is MethodCallExpression methodCallExpression)
            {
                switch (methodCallExpression.Method.Name)
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

            throw new ShardingCoreException($"query expression:[{queryCompilerContext.GetQueryExpression().ShardingPrint()}] is not terminate operate");
        }
    }
}