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
        IVirtualDataSource GetVirtualDataSource();
        List<IVirtualDataSource> GetAllVirtualDataSources();

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
        IVirtualDataSource<TShardingDbContext> GetVirtualDataSource();
        List<IVirtualDataSource<TShardingDbContext>> GetAllVirtualDataSources();
        void AddVirtualDataSource(IVirtualDataSourceConfigurationParams<TShardingDbContext> configurationParams);
    }
}
