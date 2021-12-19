using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.QueryableCombines;

namespace ShardingCore.Sharding.ShardingExecutors
{
    public class QueryCompilerContextFactory: IQueryCompilerContextFactory
    {
        private static readonly IQueryableCombine _enumeratorQueryableCombine;
        private static readonly IQueryableCombine _allQueryableCombine;
        private static readonly IQueryableCombine _constantQueryableCombine;
        private static readonly IQueryableCombine _selectQueryableCombine;
        private static readonly IQueryableCombine _whereQueryableCombine;
        static QueryCompilerContextFactory()
        {
            _enumeratorQueryableCombine = new EnumeratorQueryableCombine();
            _allQueryableCombine = new AllQueryableCombine();
            _constantQueryableCombine = new ConstantQueryableCombine();
            _selectQueryableCombine = new SelectQueryableCombine();
            _whereQueryableCombine = new WhereQueryableCombine();
    }
        public IQueryCompilerContext Create<TResult>(IShardingDbContext shardingDbContext, Expression queryExpression,bool async)
        {
            IQueryCompilerContext queryCompilerContext =
                QueryCompilerContext.Create(shardingDbContext, queryExpression);
            if (queryCompilerContext.GetQueryCompilerExecutor() is not null)
            {
                return queryCompilerContext;
            }
            var (queryEntityType, queryableCombine, isEnumerableQuery) = GetQueryableCombine<TResult>(queryCompilerContext, async);

            var dataSourceRouteRuleEngineFactory = (IDataSourceRouteRuleEngineFactory)ShardingContainer.GetService(typeof(IDataSourceRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            var tableRouteRuleEngineFactory = (ITableRouteRuleEngineFactory)ShardingContainer.GetService(typeof(ITableRouteRuleEngineFactory<>).GetGenericType0(queryCompilerContext.GetShardingDbContextType()));
            var queryCombineResult = queryableCombine.Combine(queryCompilerContext, queryEntityType);
            var dataSourceRouteResult = dataSourceRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable());
            var tableRouteResults = tableRouteRuleEngineFactory.Route(queryCombineResult.GetCombineQueryable());
            var mergeCombineCompilerContext = MergeQueryCompilerContext.Create(queryCompilerContext, queryCombineResult, queryEntityType, dataSourceRouteResult,
                tableRouteResults, isEnumerableQuery);
            return mergeCombineCompilerContext;
        }

        private (Type queryEntityType, IQueryableCombine queryableCombine,bool isEnumerableQuery) GetQueryableCombine<TResult>(IQueryCompilerContext queryCompilerContext, bool async)
        {
            var isEnumerableQuery = IsEnumerableQuery<TResult>(queryCompilerContext,async);
            if (isEnumerableQuery)
            {
                var queryEntityType = GetEnumerableQueryEntityType<TResult>(queryCompilerContext,async);
                return (queryEntityType, _enumeratorQueryableCombine, isEnumerableQuery);
            }
            else
            {
                var queryEntityType = (queryCompilerContext.GetQueryExpression() as MethodCallExpression)
                    .GetQueryEntityType();
                var queryableCombine = GetMethodQueryableCombine(queryCompilerContext);
                return (queryEntityType, queryableCombine,isEnumerableQuery);
            }
        }

        private bool IsEnumerableQuery<TResult>(IQueryCompilerContext queryCompilerContext,bool async)
        {
            return !async && queryCompilerContext.GetQueryExpression().Type
                       .HasImplementedRawGeneric(typeof(IQueryable<>))
                   ||
                   async && typeof(TResult).HasImplementedRawGeneric(typeof(IAsyncEnumerable<>));
        }

        private Type GetEnumerableQueryEntityType<TResult>(IQueryCompilerContext queryCompilerContext, bool async)
        {

            Type queryEntityType;
            if (async)
                queryEntityType = typeof(TResult).GetGenericArguments()[0];
            else
            {
                queryEntityType = queryCompilerContext.GetQueryExpression().Type.GetSequenceType();
            }

            return queryEntityType;
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
