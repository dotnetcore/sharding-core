using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerators
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:19:34
    /// Email: 326308290@qq.com
    internal class ReverseEnumeratorExecutor<TResult> : AbstractEnumeratorExecutor<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly IOrderedQueryable<TResult> _reverseOrderQueryable;
        private readonly bool _async;

        public ReverseEnumeratorExecutor(StreamMergeContext streamMergeContext, IStreamMergeCombine streamMergeCombine, IOrderedQueryable<TResult> reverseOrderQueryable, bool async) : base(streamMergeContext)
        {
            _streamMergeCombine = streamMergeCombine;
            _reverseOrderQueryable = reverseOrderQueryable;
            _async = async;
        }

        protected override  async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var connectionMode = streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
            var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;

            var shardingDbContext = streamMergeContext.CreateDbContext(dataSourceName, routeResult, connectionMode);
            var newQueryable = _reverseOrderQueryable
                .ReplaceDbContextQueryable(shardingDbContext).As<IQueryable<TResult>>();
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext,
                streamMergeAsyncEnumerator);
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return _streamMergeCombine;
        }

        public override IStreamMergeAsyncEnumerator<TResult> CombineInMemoryStreamMergeAsyncEnumerator(
            IStreamMergeAsyncEnumerator<TResult>[] streamsAsyncEnumerators)
        {
            if (GetStreamMergeContext().IsPaginationQuery() && GetStreamMergeContext().HasGroupQuery())
            {
                var multiAggregateOrderStreamMergeAsyncEnumerator = new MultiAggregateOrderStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators);
                return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), new[] { multiAggregateOrderStreamMergeAsyncEnumerator }, 0, GetStreamMergeContext().GetPaginationReWriteTake());
            }
            if (GetStreamMergeContext().IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TResult>(GetStreamMergeContext(), streamsAsyncEnumerators, 0, GetStreamMergeContext().GetPaginationReWriteTake());
            return base.CombineInMemoryStreamMergeAsyncEnumerator(streamsAsyncEnumerators);
        }
    }
}
