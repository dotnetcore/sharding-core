using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerables.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerables
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:19:34
    /// Email: 326308290@qq.com
    internal class ReverseEnumerableExecutor<TResult> : AbstractEnumerableExecutor<TResult>
    {
        private readonly IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> _shardingMerger;
        private readonly IOrderedQueryable<TResult> _reverseOrderQueryable;
        private readonly bool _async;

        public ReverseEnumerableExecutor(StreamMergeContext streamMergeContext, IOrderedQueryable<TResult> reverseOrderQueryable, bool async) : base(streamMergeContext)
        {
            _reverseOrderQueryable = reverseOrderQueryable;
            _async = async;
            _shardingMerger = new ReverseEnumerableShardingMerger<TResult>(streamMergeContext, async);
        }

        protected override  async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();

            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = _reverseOrderQueryable
                .ReplaceDbContextQueryable(shardingDbContext).As<IQueryable<TResult>>();
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
