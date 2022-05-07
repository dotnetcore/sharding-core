using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:00:40
    /// Email: 326308290@qq.com
    internal class DefaultEnumeratorExecutor<TResult> : AbstractEnumeratorExecutor<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly bool _async;

        public DefaultEnumeratorExecutor(StreamMergeContext streamMergeContext, IStreamMergeCombine streamMergeCombine, bool async) : base(streamMergeContext)
        {
            _streamMergeCombine = streamMergeCombine;
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

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var connectionMode = streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
            var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;

            var shardingDbContext = streamMergeContext.CreateDbContext(dataSourceName, routeResult, connectionMode);
            var newQueryable = (IQueryable<TResult>)streamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);

            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext, streamMergeAsyncEnumerator);
        }
    }
}
