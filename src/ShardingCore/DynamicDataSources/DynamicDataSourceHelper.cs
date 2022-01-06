using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    public class DynamicDataSourceHelper
    {
        private DynamicDataSourceHelper()
        {
            throw new InvalidOperationException($"{nameof(DynamicDataSourceHelper)} create instance");
        }

        public static void DynamicAppendDataSource<TShardingDbContext>(IVirtualDataSource<TShardingDbContext> virtualDataSource,string dataSourceName, string connectionString) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var defaultDataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
            defaultDataSourceInitializer.InitConfigure(virtualDataSource,dataSourceName, connectionString, false);
        }
        public static void DynamicAppendDataSource<TShardingDbContext>(string configId,string dataSourceName, string connectionString) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var defaultDataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
            var virtualDataSourceManager = ShardingContainer.GetService<IVirtualDataSourceManager<TShardingDbContext>>();

            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(configId);
            defaultDataSourceInitializer.InitConfigure(virtualDataSource, dataSourceName, connectionString, false);
        }

        public static void DynamicAppendVirtualDataSource<TShardingDbContext>(
            IVirtualDataSourceConfigurationParams<TShardingDbContext> configurationParams)
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSourceManager = ShardingContainer.GetRequiredVirtualDataSourceManager<TShardingDbContext>();
            if (virtualDataSourceManager.AddVirtualDataSource(configurationParams))
            {
                virtualDataSourceManager.SetDefaultIfMultiConfiguration();
                var dataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
                var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(configurationParams.ConfigId);
                foreach (var dataSource in virtualDataSource.GetDataSources())
                {
                    var dataSourceName = dataSource.Key;
                    var connectionString = dataSource.Value;
                    dataSourceInitializer.InitConfigure(virtualDataSource, dataSourceName, connectionString, false);
                }
            }
        }

    }
}
