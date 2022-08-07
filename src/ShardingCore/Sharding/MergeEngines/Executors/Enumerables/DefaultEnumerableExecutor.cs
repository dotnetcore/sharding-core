using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
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
    /// Created: 2022/5/6 13:00:40
    /// Email: 326308290@qq.com
    internal class DefaultEnumerableExecutor<TResult> : AbstractEnumerableExecutor<TResult>
    {
        // private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> _shardingMerger;
        private readonly bool _async;

        public DefaultEnumerableExecutor(StreamMergeContext streamMergeContext, bool async) : base(streamMergeContext)
        {
            // _streamMergeCombine = streamMergeCombine;
            _async = async;
            _shardingMerger = new DefaultEnumerableShardingMerger<TResult>(streamMergeContext,async);
        }
        //
        // protected override IStreamMergeCombine GetStreamMergeCombine()
        // {
        //     return _streamMergeCombine;
        // }

        // public override IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
        //     IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        // {
        //     if (GetStreamMergeContext().IsPaginationQuery())
        //         return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators, 0, GetStreamMergeContext().GetPaginationReWriteTake());//内存聚合分页不可以直接获取skip必须获取skip+take的数目
        //     return base.CombineInMemoryStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        // }

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync0(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = (IQueryable<TResult>)streamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);

            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext, streamMergeAsyncEnumerator);
        }

        public override IShardingMerger<IStreamMergeAsyncEnumerator<TResult>> GetShardingMerger()
        {
            return _shardingMerger;
        }
    }
}
