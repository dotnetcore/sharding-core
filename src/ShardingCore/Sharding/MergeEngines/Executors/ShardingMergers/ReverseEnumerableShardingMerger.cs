using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class ReverseEnumerableShardingMerger<TEntity> : AbstractEnumerableShardingMerger<TEntity>
    {
        public ReverseEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async) : base(
            streamMergeContext, async)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity> StreamMerge(
            List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            if (GetStreamMergeContext().IsPaginationQuery() && GetStreamMergeContext().HasGroupQuery())
            {
                var multiAggregateOrderStreamMergeAsyncEnumerator =
                    new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(),
                        parallelResults);
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(),
                    new[] { multiAggregateOrderStreamMergeAsyncEnumerator }, 0,
                    GetStreamMergeContext().GetPaginationReWriteTake());
            }

            if (GetStreamMergeContext().IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(), parallelResults, 0,
                    GetStreamMergeContext().GetPaginationReWriteTake());
            return base.StreamMerge(parallelResults);
        }
    }
}