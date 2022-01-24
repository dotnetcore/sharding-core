using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions.ParallelExecutors;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Base;

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal class SequenceEnumeratorParallelExecutor<TEntity>: AbstractEnumeratorParallelExecutor<TEntity>
    {
        private readonly StreamMergeContext<TEntity> _streamMergeContext;
        private readonly bool _async;
        private readonly IQueryable<TEntity> _noPaginationQueryable;

        public SequenceEnumeratorParallelExecutor(StreamMergeContext<TEntity> streamMergeContext,bool async)
        {
            _streamMergeContext = streamMergeContext;
            _async = async;
            _noPaginationQueryable = streamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake();
        }
        public override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var connectionMode = _streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var (newQueryable, dbContext) = CreateAsyncExecuteQueryable(
                ((SqlSequenceRouteUnit)sqlExecutorUnit.RouteUnit).SequenceResult, connectionMode);
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(dbContext, streamMergeAsyncEnumerator);
        }
        private (IQueryable<TEntity>, DbContext) CreateAsyncExecuteQueryable( SequenceResult sequenceResult, ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = _streamMergeContext.CreateDbContext(sequenceResult.DSName, sequenceResult.TableRouteResult, connectionMode);
            var newQueryable = (IQueryable<TEntity>)(_noPaginationQueryable.Skip(sequenceResult.Skip).Take(sequenceResult.Take))
                .ReplaceDbContextQueryable(shardingDbContext);
            return (newQueryable, shardingDbContext);
        }
    }
}
