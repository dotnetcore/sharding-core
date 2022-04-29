using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TResult, TSelect> : AbstractNoTripEnsureMethodCallInMemoryAsyncMergeEngine<TEntity, TResult>
    {
        public AverageAsyncInMemoryMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        private async Task<List<RouteQueryResult<AverageResult<T>>>> AggregateAverageResultAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {

            return (await base.ExecuteAsync(
                async queryable =>
                {
                    var count = 0L;
                    T sum = default;
                    var newQueryable = ((IQueryable<T>)queryable);
                    var r = await newQueryable.GroupBy(o => 1).BuildExpression().FirstOrDefaultAsync(cancellationToken);
                    if (r != null)
                    {
                        count = r.Item1;
                        sum = r.Item2;
                    }
                    if (count <= 0)
                    {
                        return default;
                    }
                    return new AverageResult<T>(sum, count);



                },
                cancellationToken)).Where(o => o.QueryResult != null).ToList();
        }

        public override async Task<TResult> MergeResultAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            {
                if (!typeof(TSelect).IsNumericType())
                {
                    throw new ShardingCoreException(
                        $"not support {GetStreamMergeContext().MergeQueryCompilerContext.GetQueryExpression().ShardingPrint()} result {typeof(TSelect)}");
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
                return AggregateExtension.AverageConstant<TSelect, long, TResult>(sum, count);
            }

        }

    }
}