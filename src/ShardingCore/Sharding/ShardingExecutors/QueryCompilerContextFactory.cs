using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;
using ShardingCore.Sharding.Visitors;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContextFactory : IQueryCompilerContextFactory
    {
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

        public IQueryCompilerContext Create(ICompileParameter compileParameter)
        {
            var queryCompilerContext =
                QueryCompilerContext.Create(compileParameter);
            if (queryCompilerContext.GetQueryCompilerExecutor() is not null)
            {
                return queryCompilerContext;
            }

            var queryableCombine = GetQueryableCombine(queryCompilerContext);

            var dataSourceRouteRuleEngineFactory = (IDataSourceRouteRuleEngineFactory)ShardingContainer.GetService(typeof(IDataSourceRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            var tableRouteRuleEngineFactory = (ITableRouteRuleEngineFactory)ShardingContainer.GetService(typeof(ITableRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            var queryCombineResult = queryableCombine.Combine(queryCompilerContext);
            var dataSourceRouteResult = dataSourceRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable(), compileParameter.GetShardingDbContext());
            var tableRouteResults = tableRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable());
            var routeResults = tableRouteResults as TableRouteResult[] ?? tableRouteResults.ToArray();
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