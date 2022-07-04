namespace ShardingCore.Core.ShardingMigrations.Abstractions
{
    
    public interface IShardingMigrationAccessor
    {
        ShardingMigrationContext ShardingMigrationContext { get; set; }
    }
}
