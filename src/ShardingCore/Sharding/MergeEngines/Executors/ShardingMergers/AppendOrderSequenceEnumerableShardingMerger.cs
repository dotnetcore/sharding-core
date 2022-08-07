using System.Collections.Generic;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    /// <summary>
    /// 和普通的不同因为是顺序查询所以需要忽略分页合并
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class AppendOrderSequenceEnumerableShardingMerger<TEntity> : AbstractEnumerableShardingMerger<TEntity>
    {
        public AppendOrderSequenceEnumerableShardingMerger(StreamMergeContext streamMergeContext, bool async) : base(
            streamMergeContext, async)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity> StreamMerge(
            List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            if (GetStreamMergeContext().HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(),
                    parallelResults);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(), parallelResults);
        }
    }
}