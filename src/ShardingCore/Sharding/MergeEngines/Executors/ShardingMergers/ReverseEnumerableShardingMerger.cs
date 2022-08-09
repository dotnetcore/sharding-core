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

        public override IStreamMergeAsyncEnumerator<TEntity> StreamMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            var streamMergeAsyncEnumerator = base.StreamMerge(parallelResults);
            return new InMemoryReverseStreamMergeAsyncEnumerator<TEntity>(streamMergeAsyncEnumerator);
        }
    }
}