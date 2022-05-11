using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.DynamicDataSources
{
    [Obsolete("plz use DynamicShardingHelper")]
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

        public static bool DynamicAppendVirtualDataSource<TShardingDbContext>(
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

                return true;
            }

            return false;
        }
        /// <summary>
        /// 动态添加读写分离链接字符串
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="virtualDataSource"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public static void DynamicAppendReadWriteConnectionString<TShardingDbContext>(IVirtualDataSource<TShardingDbContext> virtualDataSource, string dataSourceName,
            string connectionString,string readNodeName = null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            if (virtualDataSource.ConnectionStringManager is IReadWriteConnectionStringManager
                readWriteAppendConnectionString)
            {
                readWriteAppendConnectionString.AddReadConnectionString(dataSourceName, connectionString, readNodeName);
                return;
            }

            throw new ShardingCoreInvalidOperationException(
                $"{virtualDataSource.ConnectionStringManager.GetType()} cant support add read connection string");
        }
        /// <summary>
        /// 动态添加读写分离链接字符串
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="configId"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        public static void DynamicAppendReadWriteConnectionString<TShardingDbContext>(string configId, string dataSourceName,
            string connectionString, string readNodeName=null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSourceManager = ShardingContainer.GetRequiredVirtualDataSourceManager<TShardingDbContext>();
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(configId);
            DynamicAppendReadWriteConnectionString(virtualDataSource, dataSourceName, connectionString,readNodeName);
        }
    }
}
