using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.ShardingConfigurations.Abstractions
{
    public interface IShardingConfigurationOptions
    {
         public void AddShardingGlobalConfigOptions(ShardingGlobalConfigOptions shardingGlobalConfigOptions);

         public ShardingGlobalConfigOptions[] GetAllShardingGlobalConfigOptions();
    }

    public interface IShardingConfigurationOptions<TShardingDbContext> : IShardingConfigurationOptions where TShardingDbContext:DbContext,IShardingDbContext
    {

    }
}
