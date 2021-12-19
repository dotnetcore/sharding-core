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
    * @Date: 2021/8/18 14:40:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class MinAsyncInMemoryMergeEngine<TEntity, TSelect> : AbstractGenericMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TSelect>
    {
        public MinAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        private TResult GetMinTResult<TInnerSelect, TResult>(List<RouteQueryResult<TInnerSelect>> source)
        {
            var routeQueryResults = source.Where(o => o.QueryResult != null).ToList();
            if (routeQueryResults.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var min = routeQueryResults.Min(o => o.QueryResult);

            return ConvertMin<TResult, TInnerSelect>(min);
        }
        public override async Task<TResult> MergeResultAsync<TResult>(CancellationToken cancellationToken = new CancellationToken())
        {

            var resultType = typeof(TResult);
            if (!resultType.IsNullableType())
            {
                if (typeof(decimal) == typeof(TResult))
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<decimal>)queryable).Select(o => (decimal?)o).MinAsync(cancellationToken), cancellationToken);
                    return GetMinTResult<decimal?, TResult>(result);
                }
                if (typeof(float) == typeof(TResult))
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<float>)queryable).Select(o => (float?)o).MinAsync(cancellationToken),cancellationToken);
                    return GetMinTResult<float?, TResult>(result);
                }
                if (typeof(int) == typeof(TResult))
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<int>)queryable).Select(o => (int?)o).MinAsync(cancellationToken),
                        cancellationToken);


                    return GetMinTResult<int?, TResult>(result);
                }
                if (typeof(long) == typeof(TResult))
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<long>)queryable).Select(o => (long?)o).MinAsync(cancellationToken),
                        cancellationToken);

                    return GetMinTResult<long?, TResult>(result);
                }
                if (typeof(double) == typeof(TResult))
                {
                    var result = await base.ExecuteAsync(queryable =>
                            ((IQueryable<double>)queryable).Select(o => (double?)o).MinAsync(cancellationToken),
                        cancellationToken);
                    return GetMinTResult<double?, TResult>(result);
                }
                throw new ShardingCoreException($"cant calc min value, type:[{resultType}]");
            }
            //返回结果是否可以为空
            //可以为空
            else
            {
                var result = (await base.ExecuteAsync(queryable => ((IQueryable<TResult>)queryable).MinAsync(cancellationToken), cancellationToken))
                    .Where(o => o.QueryResult != null)
                    .ToList();
                return result.Min(o => o.QueryResult);
            }

        }
        private TResult ConvertMin<TResult, TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
            return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
        }
    }
}
