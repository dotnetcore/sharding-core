using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class LastOrDefaultEnumerableShardingMerger<TEntity> : AbstractEnumerableShardingMerger<TEntity>
    {
        public LastOrDefaultEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async) : base(
            streamMergeContext, async)
        {
        }
    }
}
