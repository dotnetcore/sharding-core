using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingSingleConfigurationOptions<TShardingDbContext> : IShardingConfigurationOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {

        private ShardingConfigOptions<TShardingDbContext> _shardingConfigOptions;
        public ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; set; } =
            ShardingConfigurationStrategyEnum.ThrowIfNull;

        public void AddShardingGlobalConfigOptions(ShardingConfigOptions<TShardingDbContext> shardingConfigOptions)
        {
            if (_shardingConfigOptions != null)
                throw new ShardingCoreInvalidOperationException($"repeat add {nameof(ShardingConfigOptions<TShardingDbContext>)}");
            _shardingConfigOptions= shardingConfigOptions;
        }

        public ShardingConfigOptions<TShardingDbContext>[] GetAllShardingGlobalConfigOptions()
        {
            return new[] { _shardingConfigOptions };
        }
    }
}
