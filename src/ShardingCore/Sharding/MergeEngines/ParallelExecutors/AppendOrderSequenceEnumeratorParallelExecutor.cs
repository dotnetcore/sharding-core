using ShardingCore.Extensions;
using ShardingCore.Extensions.InternalExtensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal class AppendOrderSequenceEnumeratorParallelExecutor<TEntity>:AbstractEnumeratorParallelExecutor<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;
        private readonly IQueryable<TEntity> _noPaginationQueryable;
        private readonly bool _async;

        public AppendOrderSequenceEnumeratorParallelExecutor(StreamMergeContext streamMergeContext, bool async)
        {
            _streamMergeContext = streamMergeContext;
            _noPaginationQueryable = streamMergeContext.GetOriginalQueryable().RemoveSkip().RemoveTake().As<IQueryable<TEntity>>();
            _async = async;
        }
        public override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var connectionMode = _streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var sequenceResult = sqlExecutorUnit.RouteUnit.As<SqlSequenceRouteUnit>().SequenceResult;
            var shardingDbContext = _streamMergeContext.CreateDbContext(sequenceResult.DSName, sequenceResult.TableRouteResult, connectionMode);
            var newQueryable = _noPaginationQueryable
                .Skip(sequenceResult.Skip)
                .Take(sequenceResult.Take)
                .OrderWithExpression(_streamMergeContext.Orders)
                .ReplaceDbContextQueryable(shardingDbContext)
                .As<IQueryable<TEntity>>();
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(shardingDbContext,streamMergeAsyncEnumerator);
        }
    }
}
