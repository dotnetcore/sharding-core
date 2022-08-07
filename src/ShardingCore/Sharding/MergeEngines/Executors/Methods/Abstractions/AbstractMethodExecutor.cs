using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions
{
    internal abstract class AbstractMethodExecutor<TResult> : AbstractExecutor<TResult>
    {
        protected AbstractMethodExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override async Task<ShardingMergeResult<TResult>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();

            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = GetStreamMergeContext().GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);

            var queryResult = await EFCoreQueryAsync(newQueryable, cancellationToken);
            await streamMergeContext.DbContextDisposeAsync(shardingDbContext);
            return new ShardingMergeResult<TResult>(null, queryResult);
        }

        protected abstract Task<TResult> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken());
    }
}
