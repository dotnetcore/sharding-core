using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.Enumerables
{
    internal class SingleOrDefaultShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
    {
        public SingleOrDefaultShardingEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
        {
            GetStreamMergeContext().ReSetTake(2);
            return new DefaultEnumerableExecutor<TEntity>(GetStreamMergeContext(), async);
        }
    }
}
