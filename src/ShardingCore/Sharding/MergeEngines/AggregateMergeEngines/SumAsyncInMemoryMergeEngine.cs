using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 14:46:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class SumAsyncInMemoryMergeEngine<TEntity, TEnsureResult> : AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult,TEnsureResult>
    {
        public SumAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression, IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        public override TEnsureResult MergeResult()
        {

            return AsyncHelper.RunSync(() => MergeResultAsync());
        }

        public override async Task<TEnsureResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(TEnsureResult);
            if(!resultType.IsNumericType())
                throw new ShardingCoreException(
                    $"not support {GetMethodCallExpression().ShardingPrint()} result {resultType}");
#if !EFCORE2
            var result = await base.ExecuteAsync(queryable => ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TEnsureResult, Task<TEnsureResult>>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TEnsureResult>)queryable, (Expression)null, cancellationToken), cancellationToken);
            return GetSumResult(result);
#endif
#if EFCORE2
            var result = await base.ExecuteAsync(queryable => ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<TEnsureResult, TEnsureResult>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<TEnsureResult>)queryable, cancellationToken), cancellationToken);
            return GetSumResult(result);
#endif

        }

        private TEnsureResult GetSumResult<TInnerSelect>(List<RouteQueryResult<TInnerSelect>> source)
        {
            if (source.IsEmpty())
                return default;
            var sum = source.AsQueryable().SumByPropertyName<TInnerSelect>(nameof(RouteQueryResult<TInnerSelect>.QueryResult));
            return ConvertSum(sum);
        }
        private TEnsureResult ConvertSum<TNumber>(TNumber number)
        {
            if (number == null)
                return default;
            var convertExpr = Expression.Convert(Expression.Constant(number), typeof(TEnsureResult));
            return Expression.Lambda<Func<TEnsureResult>>(convertExpr).Compile()();
        }
    }
}
