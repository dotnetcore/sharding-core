using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines;
using ShardingCore.Sharding.MergeEngines.ParallelControls.Enumerators;
using ShardingCore.Sharding.MergeEngines.ParallelExecutors;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Helpers;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 13:32:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class ReverseShardingEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorStreamMergeEngine<TEntity>
    {
        private readonly long _total;

        public ReverseShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext, long total) : base(streamMergeContext,new ReverseStreamMergeCombine<TEntity>())
        {
            _total = total;
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var noPaginationNoOrderQueryable = StreamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().RemoveAnyOrderBy();
            var skip = StreamMergeContext.Skip.GetValueOrDefault();
            var take = StreamMergeContext.Take.HasValue ? StreamMergeContext.Take.Value : (_total - skip);
            if (take > int.MaxValue)
                throw new ShardingCoreException($"not support take more than {int.MaxValue}");
            var realSkip = _total - take - skip;
            StreamMergeContext.ReSetSkip((int)realSkip);
            var propertyOrders = StreamMergeContext.Orders.Select(o => new PropertyOrder(o.PropertyExpression, !o.IsAsc, o.OwnerType)).ToArray();
            StreamMergeContext.ReSetOrders(propertyOrders);
            var reverseOrderQueryable = noPaginationNoOrderQueryable.Take((int)realSkip + (int)take).OrderWithExpression(propertyOrders);

            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var reverseEnumeratorParallelExecutor = new ReverseEnumeratorParallelExecutor<TEntity>(GetStreamMergeContext(), reverseOrderQueryable, async);
            var enumeratorTasks = GetDataSourceGroupAndExecutorGroup<IStreamMergeAsyncEnumerator<TEntity>>(async, defaultSqlRouteUnits, reverseEnumeratorParallelExecutor, cancellationToken);
            var streamEnumerators = TaskHelper.WhenAllFastFail(enumeratorTasks).WaitAndUnwrapException().SelectMany(o => o).ToArray();
            return streamEnumerators;
        }
        protected override IParallelExecuteControl<IStreamMergeAsyncEnumerator<TEntity>> CreateParallelExecuteControl0(IParallelExecutor<IStreamMergeAsyncEnumerator<TEntity>> executor)
        {
            return new ReverseEnumeratorParallelExecuteControl<TEntity>(GetStreamMergeContext(), executor, GetStreamMergeCombine());
        }
    }
}
