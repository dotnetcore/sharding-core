using ShardingCore.Sharding.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions
{
    internal interface IStreamMergeCombine
    {
        IStreamMergeAsyncEnumerator<TEntity> StreamMergeEnumeratorCombine<TEntity>(StreamMergeContext streamMergeContext,IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators);
    }
}
