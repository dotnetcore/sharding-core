using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.RuntimeContexts;
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
        /// 动态添加数据源
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        public static void DynamicAppendDataSource<TShardingDbContext>(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName, string connectionString,bool createDatabase,bool createTable) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
            var dataSourceInitializer = shardingRuntimeContext.GetDataSourceInitializer();
            dataSourceInitializer.InitConfigure(dataSourceName,createDatabase,createTable);
        }

        /// <summary>
        /// 动态添加读写分离链接字符串
        /// </summary>
        /// <typeparam name="TShardingDbContext"></typeparam>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public static void DynamicAppendReadWriteConnectionString<TShardingDbContext>(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName,
            string connectionString, string readNodeName=null) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            if (virtualDataSource.ConnectionStringManager is IReadWriteConnectionStringManager
                readWriteAppendConnectionString)
            {
                readWriteAppendConnectionString.AddReadConnectionString(dataSourceName, connectionString, readNodeName);
                return;
            }

            throw new ShardingCoreInvalidOperationException(
                $"{virtualDataSource.ConnectionStringManager.GetType()} cant support add read connection string");
        }
    }
}
