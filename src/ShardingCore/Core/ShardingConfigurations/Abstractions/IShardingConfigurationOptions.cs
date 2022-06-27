using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingConfigurationOptions
    {
        public void AddShardingGlobalConfigOptions(ShardingConfigOptions shardingConfigOptions);

        public ShardingConfigOptions[] GetAllShardingGlobalConfigOptions();
    }
}
