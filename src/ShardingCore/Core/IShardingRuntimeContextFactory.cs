namespace ShardingCore.Core
{
    public interface IShardingRuntimeContextFactory
    {
        IShardingRuntimeContext Create();
    }
}