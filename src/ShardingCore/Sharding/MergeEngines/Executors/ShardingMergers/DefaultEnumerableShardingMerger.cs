using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class DefaultEnumerableShardingMerger<TEntity>:AbstractEnumerableShardingMerger<TEntity>
    {
        public DefaultEnumerableShardingMerger(StreamMergeContext streamMergeContext,bool async) : base(streamMergeContext,async)
        {
        }
    } 
}
