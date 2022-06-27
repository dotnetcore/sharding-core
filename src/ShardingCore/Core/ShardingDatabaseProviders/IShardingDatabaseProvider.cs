using System;

namespace ShardingCore.Core.ShardingDatabaseProviders
{
    
    public interface IShardingDatabaseProvider
    {
        Type GetShardingDbContextType();
    }
}
