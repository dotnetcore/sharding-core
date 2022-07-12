using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources;
using ShardingCore.Exceptions;
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
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="createDatabase"></param>
        /// <param name="createTable"></param>
        public static void DynamicAppendDataSource(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName, string connectionString,bool createDatabase,bool createTable)
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
            var dataSourceInitializer = shardingRuntimeContext.GetDataSourceInitializer();
            dataSourceInitializer.InitConfigure(dataSourceName,createDatabase,createTable);
        }
        /// <summary>
        /// 动态添加数据源
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        public static void DynamicAppendDataSourceOnly(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName, string connectionString)
        {
            var virtualDataSource = shardingRuntimeContext.GetVirtualDataSource();
            virtualDataSource.AddPhysicDataSource(new DefaultPhysicDataSource(dataSourceName, connectionString, false));
        }

        /// <summary>
        /// 动态添加读写分离链接字符串
        /// </summary>
        /// <param name="shardingRuntimeContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <exception cref="ShardingCoreInvalidOperationException"></exception>
        public static void DynamicAppendReadWriteConnectionString(IShardingRuntimeContext shardingRuntimeContext, string dataSourceName,
            string connectionString, string readNodeName=null)
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
