using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
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
    * @Date: 2021/8/18 14:46:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SumAsyncInMemoryMergeEngine<TEntity, TResult> : AbstractNoTripEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        public SumAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(TResult);
            if(!resultType.IsNumericType())
                throw new ShardingCoreException(
                    $"not support {GetStreamMergeContext().MergeQueryCompilerContext.GetQueryExpression().ShardingPrint()} result {resultType}");
#if !EFCORE2
            var result = await base.ExecuteAsync(queryable => ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TResult, Task<TResult>>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TResult>)queryable, (Expression)null, cancellationToken), cancellationToken);
#endif
#if EFCORE2
            var result = await base.ExecuteAsync(queryable => ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TResult, TResult>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TResult>)queryable, cancellationToken), cancellationToken);
#endif
            return GetSumResult(result);
        }

        private TResult GetSumResult<TInnerSelect>(List<RouteQueryResult<TInnerSelect>> source)
        {
            if (source.IsEmpty())
                return default;
            var sum = source.AsQueryable().SumByPropertyName<TInnerSelect>(nameof(RouteQueryResult<TInnerSelect>.QueryResult));
            return ConvertSum(sum);
        }
        private TResult ConvertSum<TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TResult));
            return Expression.Lambda<Func<TResult>>(convertExpr).Compile()();
        }
    }
}
