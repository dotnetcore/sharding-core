using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables
{
    internal class LastOrDefaultEnumerableExecutor<TResult> : AbstractEnumerableExecutor<TResult>
    {
        private readonly IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> _shardingMerger;
        private readonly IQueryable<TResult> _queryable;
        private readonly bool _async;

        public LastOrDefaultEnumerableExecutor(StreamMergeContext streamMergeContext, IQueryable<TResult> queryable,
            bool async) : base(streamMergeContext)
        {
            _queryable = queryable;
            _async = async;
            _shardingMerger = new LastOrDefaultEnumerableShardingMerger<TResult>(GetStreamMergeContext(), async);
        }


        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(
            SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();

            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = (IQueryable<TResult>)_queryable.ReplaceDbContextQueryable(shardingDbContext);

            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext,
                streamMergeAsyncEnumerator);
        }

        public override IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> GetShardingMerger()
        {
            return _shardingMerger;
        }
    }
}