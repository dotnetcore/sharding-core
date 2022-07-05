using System.Linq;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    internal class LastOrDefaultStreamEnumerable<TEntity> : AbstractStreamEnumerable<TEntity>
    {
        public LastOrDefaultStreamEnumerable(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return DefaultStreamMergeCombine.Instance;
        }

        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            var streamMergeContext = GetStreamMergeContext();
            var skip = streamMergeContext.Skip;
            streamMergeContext.ReverseOrder();
            streamMergeContext.ReSetSkip(0);
            var reTake = skip.GetValueOrDefault() + 1;
            streamMergeContext.ReSetTake(reTake);
            var newQueryable = (IQueryable<TEntity>)streamMergeContext.GetReWriteQueryable().RemoveSkip().RemoveTake().RemoveAnyOrderBy().OrderWithExpression(streamMergeContext.Orders).ReTake(reTake);

            return new LastOrDefaultEnumeratorExecutor<TEntity>(GetStreamMergeContext(), GetStreamMergeCombine(), newQueryable, async);
        }
    }
}