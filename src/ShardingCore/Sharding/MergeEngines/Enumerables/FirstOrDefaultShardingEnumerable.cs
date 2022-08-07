using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.Enumerables
{

    internal class FirstOrDefaultShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
    {
        public FirstOrDefaultShardingEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
        {
            GetStreamMergeContext().ReSetTake(1);
            return new DefaultEnumerableExecutor<TEntity>(GetStreamMergeContext(), async);
        }
    }
}
