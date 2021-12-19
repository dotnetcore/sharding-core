using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractGenericMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:44:53
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MaxAsyncInMemoryMergeEngine<TEntity, TSelect> : AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TSelect>
    {
        public MaxAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }
        private TResult GetMaxTResult<TInnerSelect, TResult>(List<RouteQueryResult<TInnerSelect>> source)
        {
            var routeQueryResults = source.Where(o => o.QueryResult != null).ToList();
            if (routeQueryResults.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var max = routeQueryResults.Max(o => o.QueryResult);

            return ConvertMax<TResult, TInnerSelect>(max);
        }
        public override async Task<TResult> MergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(TResult);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == resultType)
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<decimal>)queryable).Select(o => (decimal?)o).MaxAsync(cancellationToken), cancellationToken);
                    return GetMaxTResult<decimal?, TResult>(result);
                }
                if (typeof(float) == resultType)
                {
                    var result = await base.ExecuteAsync(queryable =>
                        ((IQueryable<float>)queryable).Select(o => (float?)o).MaxAsync(cancellationToken), cancellationToken);

                    return GetMaxTResult<float?, TResult>(result);
                }
                if (typeof(int) == resultType)
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<int>)queryable).Select(o => (int?)o).MaxAsync(cancellationToken),cancellationToken);
                    return GetMaxTResult<int?, TResult>(result);
                }
                if (typeof(long) == resultType)
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<long>)queryable).Select(o => (long?)o).MaxAsync(cancellationToken), cancellationToken);

                    return GetMaxTResult<long?, TResult>(result);
                }
                if (typeof(double) == resultType)
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<double>)queryable).Select(o => (double?)o).MaxAsync(cancellationToken), cancellationToken);
                    return GetMaxTResult<double?, TResult>(result);
                }

                throw new ShardingCoreException($"cant calc max value, type:[{resultType}]");
            }
            else
            {

                var result = (await base.ExecuteAsync(queryable => ((IQueryable<TResult>)queryable).MaxAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                return result.Max(o => o.QueryResult);
            }
        }

        private TSum ConvertMax<TSum, TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TSum));
            return Expression.Lambda<Func<TSum>>(convertExpr).Compile()();
        }
    }
}
