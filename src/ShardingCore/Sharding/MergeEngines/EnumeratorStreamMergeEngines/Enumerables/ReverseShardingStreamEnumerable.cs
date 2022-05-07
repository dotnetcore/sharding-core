using System.Linq;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;
using ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.StreamMergeCombines;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.Enumerables
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/3 13:32:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class ReverseShardingStreamEnumerable<TEntity> : AbstractStreamEnumerable<TEntity>
    {
        private readonly long _total;

        public ReverseShardingStreamEnumerable(StreamMergeContext streamMergeContext, long total) : base(streamMergeContext)
        {
            _total = total;
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return ReverseStreamMergeCombine.Instance;
        }


        protected override IExecutor<IStreamMergeAsyncEnumerator<TEntity>> CreateExecutor0(bool async)
        {
            var noPaginationNoOrderQueryable = GetStreamMergeContext().GetOriginalQueryable().RemoveSkip().RemoveTake().RemoveAnyOrderBy().As<IQueryable<TEntity>>();
            var skip = GetStreamMergeContext().Skip.GetValueOrDefault();
            var take = GetStreamMergeContext().Take.HasValue ? GetStreamMergeContext().Take.Value : (_total - skip);
            if (take > int.MaxValue)
                throw new ShardingCoreException($"not support take more than {int.MaxValue}");
            var realSkip = _total - take - skip;
            GetStreamMergeContext().ReSetSkip((int)realSkip);
            var propertyOrders = GetStreamMergeContext().Orders.Select(o => new PropertyOrder(o.PropertyExpression, !o.IsAsc, o.OwnerType)).ToArray();
            GetStreamMergeContext().ReSetOrders(propertyOrders);
            var reverseOrderQueryable = noPaginationNoOrderQueryable.Take((int)realSkip + (int)take).OrderWithExpression(propertyOrders);
            return new ReverseEnumeratorExecutor<TEntity>(GetStreamMergeContext(), GetStreamMergeCombine(),
                reverseOrderQueryable, async);
        }
    }
}
