using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge.AbstractEnsureMergeEngines;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;
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
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TEnsureResult,TSelect> :
            AbstractEnsureMethodCallSelectorInMemoryAsyncMergeEngine<TEntity, TEnsureResult, TSelect>
    {
        public AverageAsyncInMemoryMergeEngine(MethodCallExpression methodCallExpression,
            IShardingDbContext shardingDbContext) : base(methodCallExpression, shardingDbContext)
        {
        }

        private async Task<List<RouteQueryResult<AverageResult<T>>>> AggregateAverageResultAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            return (await base.ExecuteAsync(
                async queryable =>
                {
                    var count = await ((IQueryable<T>)queryable).LongCountAsync(cancellationToken);
                    if (count <= 0)
                    {
                        return default;
                    }

                    var sum = await GetSumAsync<T>(queryable,cancellationToken);
                    return new AverageResult<T>(sum, count);

                },
                cancellationToken)).Where(o => o.QueryResult != null).ToList();
        }
        
        private async Task<T> GetSumAsync<T>(IQueryable queryable,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var resultType = typeof(T);
            if (!resultType.IsNumericType())
                throw new ShardingCoreException(
                    $"not support {GetMethodCallExpression().ShardingPrint()} result {resultType}");
#if !EFCORE2
            return await ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<T, Task<T>>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<T>)queryable, (Expression)null, cancellationToken);
#endif
#if EFCORE2
            return await ShardingEntityFrameworkQueryableExtensions.ExecuteAsync<T, T>(ShardingQueryableMethods.GetSumWithoutSelector(resultType), (IQueryable<T>)queryable, cancellationToken);
#endif
            
        }
        public override async Task<TEnsureResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            {
                if (!typeof(TSelect).IsNumericType())
                {
                    throw new ShardingCoreException(
                        $"not support {GetMethodCallExpression().ShardingPrint()} result {typeof(TSelect)}");
                }
                var result = await AggregateAverageResultAsync<TSelect>(cancellationToken);
                if (result.IsEmpty())
                    throw new InvalidOperationException("Sequence contains no elements.");
                var queryable = result.Select(o => new
                {
                    Sum = o.QueryResult.Sum,
                    Count = o.QueryResult.Count
                }).AsQueryable();
                var sum = queryable.SumByPropertyName<TSelect>(nameof(AverageResult<object>.Sum));
                var count = queryable.Sum(o => o.Count);
                return AggregateExtension.AverageConstant<TSelect, long, TEnsureResult>(sum, count);
            }
            
        }
    }
}