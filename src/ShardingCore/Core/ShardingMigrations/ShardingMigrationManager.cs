using ShardingCore.Core.ShardingMigrations.Abstractions;

namespace ShardingCore.Core.ShardingMigrations
{
    public class ShardingMigrationManager:IShardingMigrationManager
    {
        private readonly IShardingMigrationAccessor _shardingMigrationAccessor;

        public ShardingMigrationManager(IShardingMigrationAccessor shardingMigrationAccessor)
        {
            _shardingMigrationAccessor = shardingMigrationAccessor;
        }

        public ShardingMigrationContext Current => _shardingMigrationAccessor.ShardingMigrationContext;
        public ShardingMigrationScope CreateScope()
        {
            var shardingMigrationScope = new ShardingMigrationScope(_shardingMigrationAccessor);
            _shardingMigrationAccessor.ShardingMigrationContext = ShardingMigrationContext.Create();
            return shardingMigrationScope;
        }
    }
}
