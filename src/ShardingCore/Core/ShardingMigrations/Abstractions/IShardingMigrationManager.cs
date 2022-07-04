namespace ShardingCore.Core.ShardingMigrations.Abstractions
{
    
    public interface IShardingMigrationManager
    {
        ShardingMigrationContext Current { get; }
        /// <summary>
        /// 创建路由scope
        /// </summary>
        /// <returns></returns>
        ShardingMigrationScope CreateScope();
    }
}
