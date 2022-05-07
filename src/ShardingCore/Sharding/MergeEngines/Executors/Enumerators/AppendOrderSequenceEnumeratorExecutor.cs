using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Enumerators.Abstractions;

namespace ShardingCore.Sharding.MergeEngines.Executors.Enumerators
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/6 13:12:31
    /// Email: 326308290@qq.com
    internal class AppendOrderSequenceEnumeratorExecutor<TResult> : AbstractEnumeratorExecutor<TResult>
    {
        private readonly IStreamMergeCombine _streamMergeCombine;
        private readonly IQueryable<TResult> _noPaginationQueryable;
        private readonly bool _async;

        public AppendOrderSequenceEnumeratorExecutor(StreamMergeContext streamMergeContext, IStreamMergeCombine streamMergeCombine, bool async) : base(streamMergeContext)
        {
            _streamMergeCombine = streamMergeCombine;
            _noPaginationQueryable = streamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().As<IQueryable<TResult>>();
            _async = async;
        }

        protected override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var connectionMode = streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var sequenceResult = sqlExecutorUnit.RouteUnit.As<SqlSequenceRouteUnit>().SequenceResult;
            var shardingDbContext = streamMergeContext.CreateDbContext(sequenceResult.DSName, sequenceResult.TableRouteResult, connectionMode);
            var newQueryable = _noPaginationQueryable
                .Skip(sequenceResult.Skip)
                .Take(sequenceResult.Take)
                .OrderWithExpression(streamMergeContext.Orders)
                .ReplaceDbContextQueryable(shardingDbContext)
                .As<IQueryable<TResult>>();
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TResult>>(shardingDbContext, streamMergeAsyncEnumerator);
        }

        protected override IStreamMergeCombine GetStreamMergeCombine()
        {
            return _streamMergeCombine;
        }
    }
}
