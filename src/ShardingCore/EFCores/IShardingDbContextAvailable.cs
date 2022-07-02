using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    
    public interface IShardingDbContextAvailable
    {
        IShardingDbContext GetShardingDbContext();
    }
}
