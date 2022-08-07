using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.Enumerables
{
    
    internal class DefaultShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
    {
        public DefaultShardingEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
        {
           return new DefaultEnumerableExecutor<TEntity>(GetStreamMergeContext(), async);
        }
    }
}
