using System;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingDatabaseProviders
{
    public class ShardingDatabaseProvider<TShardingDbContext> : IShardingDatabaseProvider
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Type _shardingDbContextType;

        public ShardingDatabaseProvider()
        {
            _shardingDbContextType = typeof(TShardingDbContext);
        }

        public Type GetShardingDbContextType()
        {
            return _shardingDbContextType;
        }
    }
}