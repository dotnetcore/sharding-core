#if EFCORE7
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{
    internal class ExecuteDeleteAsyncMemoryMergeEngine<TEntity> : AbstractMethodEnsureWrapMergeEngine<int>
    {
        public ExecuteDeleteAsyncMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }
        protected override IExecutor<RouteQueryResult<int>> CreateExecutor()
        {
            return new ExecuteDeleteMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}

#endif
