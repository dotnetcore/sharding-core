using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Common;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions
{
    public interface IVirtualDataSourceManager
    {
        bool IsMultiShardingConfiguration { get; }
        ShardingConfigurationStrategyEnum ShardingConfigurationStrategy { get; }
        IVirtualDataSource GetCurrentVirtualDataSource();
        IVirtualDataSource GetVirtualDataSource(string  configId);
        List<IVirtualDataSource> GetAllVirtualDataSources();
        bool ContansConfigId(string configId);

        /// <summary>
        /// 创建分片配置scope
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        VirtualDataSourceScope CreateScope(string configId);

    }

    public interface IVirtualDataSourceManager<TShardingDbContext> : IVirtualDataSourceManager
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        IVirtualDataSource<TShardingDbContext> GetCurrentVirtualDataSource();
        IVirtualDataSource<TShardingDbContext> GetVirtualDataSource(string configId);
        List<IVirtualDataSource<TShardingDbContext>> GetAllVirtualDataSources();
        bool AddVirtualDataSource(IVirtualDataSourceConfigurationParams<TShardingDbContext> configurationParams);
        void SetDefaultIfMultiConfiguration();
    }
}
