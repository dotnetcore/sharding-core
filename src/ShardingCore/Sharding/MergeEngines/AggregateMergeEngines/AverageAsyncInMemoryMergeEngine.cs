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
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;

namespace ShardingCore.Sharding.StreamMergeEngines.AggregateMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TResult, TSelect> : AbstractEnsureMethodCallInMemoryAverageAsyncMergeEngine<TEntity, TResult>
    {
        public AverageAsyncInMemoryMergeEngine(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }


        public override async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var resultList = await base.ExecuteAsync<AverageResult<TSelect>>(cancellationToken);
            if (resultList.IsEmpty())
                throw new InvalidOperationException("Sequence contains no elements.");
            var queryable = resultList.Where(o=>o.HasQueryResult()).Select(o => new
            {
                Sum = o.QueryResult.Sum,
                Count = o.QueryResult.Count
            }).AsQueryable();
            var sum = queryable.SumByPropertyName<TSelect>(nameof(AverageResult<object>.Sum));
            var count = queryable.Sum(o => o.Count);
            return AggregateExtension.AverageConstant<TSelect, long, TResult>(sum, count);
        }

        protected override IExecutor<TR> CreateExecutor<TR>(bool async)
        {
            return new AverageMethodExecutor<TSelect>(GetStreamMergeContext()) as IExecutor<TR>;
        }
    }
}