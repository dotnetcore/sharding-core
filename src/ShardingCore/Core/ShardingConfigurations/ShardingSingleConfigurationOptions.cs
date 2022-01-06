using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingSingleConfigurationOptions<TShardingDbContext> : IShardingConfigurationOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {

        private ShardingGlobalConfigOptions _shardingGlobalConfigOptions;
        public ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; set; } =
            ShardingConfigurationStrategyEnum.ThrowIfNull;

        public void AddShardingGlobalConfigOptions(ShardingGlobalConfigOptions shardingGlobalConfigOptions)
        {
            if (_shardingGlobalConfigOptions != null)
                throw new ShardingCoreInvalidOperationException($"repeat add {nameof(ShardingGlobalConfigOptions)}");
            _shardingGlobalConfigOptions= shardingGlobalConfigOptions;
        }

        public ShardingGlobalConfigOptions[] GetAllShardingGlobalConfigOptions()
        {
            return new[] { _shardingGlobalConfigOptions };
        }
    }
}
