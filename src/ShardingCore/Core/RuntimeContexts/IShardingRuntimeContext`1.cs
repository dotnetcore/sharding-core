using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.RuntimeContexts
{
    public interface IShardingRuntimeContext<TDbContext> : IShardingRuntimeContext where TDbContext : IShardingDbContext
    {

    }
}
