using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerators
{
internal class LastOrDefaultEnumeratorExecutor<TResult> : AbstractEnumeratorExecutor<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly IQueryable<TResult> _queryable;
        private readonly bool _async;

        public LastOrDefaultEnumeratorExecutor(StreamMergeContext streamMergeContext, IStreamMergeCombine streamMergeCombine,IQueryable<TResult> queryable, bool async) : base(streamMergeContext)
        {
            _streamMergeCombine = streamMergeCombine;
            _queryable = queryable;
            _async = async;
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return _streamMergeCombine;
        }

        public override IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            if (GetStreamMergeContext().IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators, 0, GetStreamMergeContext().GetPaginationReWriteTake());//内存聚合分页不可以直接获取skip必须获取skip+take的数目
            return base.CombineInMemoryStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        }

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();

            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = (IQueryable<TResult>)_queryable.ReplaceDbContextQueryable(shardingDbContext);

            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext, streamMergeAsyncEnumerator);
        }
    }
}
