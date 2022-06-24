using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.DynamicDataSources;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Helpers
{
    public class DynamicShardingHelper
    {
        private DynamicShardingHelper()
        {
            throw new InvalidOperationException($"{nameof(DynamicShardingHelper)} create instance");
        }
        /// <summary>
        /// 动态添加虚拟数据源配置
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="configurationParams"></param>
        /// <returns></returns>
        public static bool DynamicAppendVirtualDataSourceConfig<TShardingDbContext>(
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
        /// 动态添加数据源
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="virtualDataSource"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        public static void DynamicAppendDataSource<TShardingDbContext>(IVirtualDataSource<TShardingDbContext> virtualDataSource, string dataSourceName, string connectionString,bool? createDatabase=null,bool? createTable=null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var defaultDataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
            defaultDataSourceInitializer.InitConfigure(virtualDataSource, dataSourceName, connectionString, false,createDatabase,createTable);
        }
        /// <summary>
        /// 动态添加数据源
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="configId"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        public static void DynamicAppendDataSource<TShardingDbContext>(string configId, string dataSourceName, string connectionString, bool? createDatabase = null, bool? createTable = null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var defaultDataSourceInitializer = ShardingContainer.GetService<IDataSourceInitializer<TShardingDbContext>>();
            var virtualDataSourceManager = ShardingContainer.GetService<IVirtualDataSourceManager<TShardingDbContext>>();

            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(configId);
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
            defaultDataSourceInitializer.InitConfigure(virtualDataSource, dataSourceName, connectionString, false, createDatabase, createTable);
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
            string connectionString, string readNodeName=null) where TShardingDbContext : DbContext, IShardingDbContext
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
            string connectionString, string readNodeName = null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSourceManager = ShardingContainer.GetRequiredVirtualDataSourceManager<TShardingDbContext>();
            var virtualDataSource = virtualDataSourceManager.GetVirtualDataSource(configId);
            DynamicAppendReadWriteConnectionString(virtualDataSource, dataSourceName, connectionString, readNodeName);
        }
    }
}
