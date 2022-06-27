namespace ShardingCore.Core
{
    public interface IShardingEntityType
    {
        string GetLogicTableName();
        bool IsSingleKey { get; }
    }
}
