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
    public class ShardingMultiConfigurationOptions<TShardingDbContext> : IShardingConfigurationOptions<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        public ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; set; } =
            ShardingConfigurationStrategyEnum.ThrowIfNull;

        private Dictionary<string, ShardingGlobalConfigOptions> _shardingGlobalConfigOptions = new ();

        public void AddShardingGlobalConfigOptions(ShardingGlobalConfigOptions shardingGlobalConfigOptions)
        {
            if (_shardingGlobalConfigOptions.ContainsKey(shardingGlobalConfigOptions.ConfigId))
                throw new ShardingCoreInvalidOperationException($"repeat add config id:[{shardingGlobalConfigOptions.ConfigId}]");

            _shardingGlobalConfigOptions.Add(shardingGlobalConfigOptions.ConfigId, shardingGlobalConfigOptions);
        }

        public ShardingGlobalConfigOptions[] GetAllShardingGlobalConfigOptions()
        {
            return _shardingGlobalConfigOptions.Values.ToArray();
        }

    }
}
