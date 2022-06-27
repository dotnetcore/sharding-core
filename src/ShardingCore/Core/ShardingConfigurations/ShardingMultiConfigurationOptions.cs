using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingMultiConfigurationOptions : IShardingConfigurationOptions
    {
        public ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; set; } =
            ShardingConfigurationStrategyEnum.ThrowIfNull;

        private Dictionary<string, ShardingConfigOptions> _shardingGlobalConfigOptions = new ();

        public void AddShardingGlobalConfigOptions(ShardingConfigOptions shardingConfigOptions)
        {
            if (_shardingGlobalConfigOptions.ContainsKey(shardingConfigOptions.ConfigId))
                throw new ShardingCoreInvalidOperationException($"repeat add config id:[{shardingConfigOptions.ConfigId}]");

            _shardingGlobalConfigOptions.Add(shardingConfigOptions.ConfigId, shardingConfigOptions);
        }

        public ShardingConfigOptions[] GetAllShardingGlobalConfigOptions()
        {
            return _shardingGlobalConfigOptions.Values.ToArray();
        }

    }
}
