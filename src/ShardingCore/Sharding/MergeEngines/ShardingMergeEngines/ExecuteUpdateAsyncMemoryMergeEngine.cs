#if EFCORE7
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.Executors.Methods;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions.InMemoryMerge;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.ShardingMergeEngines
{

    internal class ExecuteUpdateAsyncMemoryMergeEngine<TEntity> : AbstractMethodEnsureWrapMergeEngine<int>
    {
        public ExecuteUpdateAsyncMemoryMergeEngine(StreamMergeContext streamStreamMergeContext) : base(streamStreamMergeContext)
        {
        }
        protected override IExecutor<RouteQueryResult<int>> CreateExecutor()
        {
            return new ExecuteUpdateMethodExecutor<TEntity>(GetStreamMergeContext());
        }
    }
}

#endif