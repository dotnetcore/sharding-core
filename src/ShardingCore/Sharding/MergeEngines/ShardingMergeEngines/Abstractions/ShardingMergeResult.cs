using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions
{
    internal class ShardingMergeResult<TResult>
    {
        public DbContext DbContext { get; }
        public TResult MergeResult { get; }

        public ShardingMergeResult(DbContext dbContext,TResult mergeResult)
        {
            DbContext = dbContext;
            MergeResult = mergeResult;
        }
    }
}
