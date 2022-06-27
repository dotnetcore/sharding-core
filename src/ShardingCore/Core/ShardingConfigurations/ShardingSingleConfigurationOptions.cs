using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingSingleConfigurationOptions : IShardingConfigurationOptions
    {

        private ShardingConfigOptions _shardingConfigOptions;
        public ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; set; } =
            ShardingConfigurationStrategyEnum.ThrowIfNull;

        public void AddShardingGlobalConfigOptions(ShardingConfigOptions shardingConfigOptions)
        {
            if (_shardingConfigOptions != null)
                throw new ShardingCoreInvalidOperationException($"repeat add {nameof(ShardingConfigOptions)}");
            _shardingConfigOptions= shardingConfigOptions;
        }

        public ShardingConfigOptions[] GetAllShardingGlobalConfigOptions()
        {
            return new[] { _shardingConfigOptions };
        }
    }
}
