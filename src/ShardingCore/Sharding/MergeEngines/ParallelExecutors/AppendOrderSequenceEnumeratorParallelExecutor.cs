using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeContexts;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base;

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal class AppendOrderSequenceEnumeratorParallelExecutor<TEntity>:AbstractEnumeratorParallelExecutor<TEntity>
    {
        private readonly StreamMergeContext<TEntity> _streamMergeContext;
        private readonly IQueryable<TEntity> _noPaginationQueryable;
        private readonly bool _async;

        public AppendOrderSequenceEnumeratorParallelExecutor(StreamMergeContext<TEntity> streamMergeContext, bool async)
        {
            _streamMergeContext = streamMergeContext;
            _noPaginationQueryable = streamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().As<IQueryable<TEntity>>(); ;
            _async = async;
        }
        public override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var connectionMode = _streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var (newQueryable, dbContext) = CreateAsyncExecuteQueryable(((SqlSequenceRouteUnit)sqlExecutorUnit.RouteUnit).SequenceResult, _streamMergeContext.Orders, connectionMode);
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(dbContext,
                streamMergeAsyncEnumerator);
        }
        private (IQueryable<TEntity>, DbContext) CreateAsyncExecuteQueryable(SequenceResult sequenceResult, IEnumerable<PropertyOrder> reSetOrders, ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = _streamMergeContext.CreateDbContext(sequenceResult.DSName, sequenceResult.TableRouteResult, connectionMode);
            var newQueryable = _noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take).OrderWithExpression(reSetOrders)
                .ReplaceDbContextQueryable(shardingDbContext).As<IQueryable<TEntity>>();
            return (newQueryable, shardingDbContext);
        }
    }
}
