using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class SequenceEnumerableShardingMerger<TEntity> : AbstractEnumerableShardingMerger<TEntity>
    {
        public SequenceEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async) : base(
            streamMergeContext, async)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity> StreamMerge(
            List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            if (GetStreamMergeContext().HasGroupQuery())
            {
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(),
                    parallelResults);
            }

            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(), parallelResults);
        }

        protected override IStreamMergeAsyncEnumerator<TEntity> StreamInMemoryMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            return StreamMerge(parallelResults);
        }
    }
}