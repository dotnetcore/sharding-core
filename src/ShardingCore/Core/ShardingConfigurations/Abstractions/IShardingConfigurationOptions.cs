using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingConfigurationOptions<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        public void AddShardingGlobalConfigOptions(ShardingConfigOptions<TShardingDbContext> shardingConfigOptions);

        public ShardingConfigOptions<TShardingDbContext>[] GetAllShardingGlobalConfigOptions();
    }
}
