using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class DefaultEnumerableShardingMerger<TEntity>:AbstractEnumerableShardingMerger<TEntity>
    {
        public DefaultEnumerableShardingMerger(StreamMergeContext streamMergeContext,bool async) : base(streamMergeContext,async)
        {
        }
    } 
}