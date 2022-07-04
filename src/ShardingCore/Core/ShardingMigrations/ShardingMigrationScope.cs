using System;
using ShardingCore.Core.ShardingMigrations.Abstractions;

namespace ShardingCore.Core.ShardingMigrations
{

    public class ShardingMigrationScope:IDisposable
    {
        private readonly IShardingMigrationAccessor _shardingMigrationAccessor;

        public ShardingMigrationScope(IShardingMigrationAccessor shardingMigrationAccessor)
        {
            _shardingMigrationAccessor = shardingMigrationAccessor;
        }
        public void Dispose()
        {
            _shardingMigrationAccessor.ShardingMigrationContext = null;
        }
    } 
}