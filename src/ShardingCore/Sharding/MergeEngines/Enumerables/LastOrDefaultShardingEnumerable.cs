using System.Linq;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.Enumerables
{
    internal class LastOrDefaultShardingEnumerable<TEntity> :AbstractStreamEnumerable<TEntity>
    {
        public LastOrDefaultShardingEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor(bool async)
        {
            var streamMergeContext = GetStreamMergeContext();
            var skip = streamMergeContext.Skip;
            streamMergeContext.ReverseOrder();
            streamMergeContext.ReSetSkip(0);
            var reTake = skip.GetValueOrDefault() + 1;
            streamMergeContext.ReSetTake(reTake);
            var newQueryable = (IQueryable<TEntity>)streamMergeContext.GetReWriteQueryable().RemoveSkip().RemoveTake().RemoveAnyOrderBy().OrderWithExpression(streamMergeContext.Orders).ReTake(reTake);

            return new LastOrDefaultEnumerableExecutor<TEntity>(GetStreamMergeContext(),newQueryable, async);
        }
    }
}
