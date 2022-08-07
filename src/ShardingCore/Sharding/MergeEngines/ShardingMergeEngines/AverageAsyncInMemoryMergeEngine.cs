using System;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators.AggregateExtensions;
using ShardingCore.Sharding.MergeEngines.AggregateMergeEngines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingExecutors;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/18 22:15:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class AverageAsyncInMemoryMergeEngine<TEntity, TResult, TSelect> : AbstractBaseMergeEngine,IEnsureMergeResult<TResult>
    {
        public AverageAsyncInMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }


        protected  IExecutor<RouteQueryResult<AverageResult<TSelect>>> CreateExecutor()
        {
            return new AverageMethodWrapExecutor<TSelect>(GetStreamMergeContext());
        }

        public TResult MergeResult()
        {
            return MergeResultAsync().WaitAndUnwrapException(false);
        }

        public  async Task<TResult> MergeResultAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!GetStreamMergeContext().TryPrepareExecuteContinueQuery(() => default(TResult),out var emptyQueryEnumerator))
            {
                return emptyQueryEnumerator;
            }
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var executor = CreateExecutor();
            var result =await ShardingExecutor.Instance.ExecuteAsync(GetStreamMergeContext(),executor,true,defaultSqlRouteUnits,cancellationToken);
            var sum = result.QueryResult.Sum;
            var count = result.QueryResult.Count;
            // var resultList = await base.ExecuteAsync<AverageResult<TSelect>>(cancellationToken);
            // if (resultList.IsEmpty())
            //     throw new InvalidOperationException("Sequence contains no elements.");
            // var queryable = resultList.Where(o=>o.HasQueryResult()).Select(o => new
            // {
            //     Sum = o.QueryResult.Sum,
            //     Count = o.QueryResult.Count
            // }).AsQueryable();
            // var sum = queryable.SumByPropertyName<TSelect>(nameof(AverageResult<object>.Sum));
            // var count = queryable.Sum(o => o.Count);
            return AggregateExtension.AverageConstant<TSelect, long, TResult>(sum, count);
        }

    }
}