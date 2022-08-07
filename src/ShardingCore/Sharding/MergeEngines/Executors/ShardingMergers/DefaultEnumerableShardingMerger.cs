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

        protected override IStreamMergeAsyncEnumerator<TEntity> StreamInMemoryMerge(List<IStreamMergeAsyncEnumerator<TEntity>> parallelResults)
        {
            if (GetStreamMergeContext().IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(GetStreamMergeContext(), parallelResults, 0, GetStreamMergeContext().GetPaginationReWriteTake());//内存聚合分页不可以直接获取skip必须获取skip+take的数目

            return base.StreamInMemoryMerge(parallelResults);
        }
    } 
}
