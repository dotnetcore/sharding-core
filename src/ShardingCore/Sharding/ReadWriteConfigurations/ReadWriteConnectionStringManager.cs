using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 10:37:28
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ReadWriteConnectionStringManager: IConnectionStringManager, IReadWriteConnectionStringManager
    {
        private IShardingConnectionStringResolver _shardingConnectionStringResolver;
        private readonly IVirtualDataSource _virtualDataSource;


        public ReadWriteConnectionStringManager(IVirtualDataSource virtualDataSource,IReadWriteConnectorFactory readWriteConnectorFactory)
        {
            _virtualDataSource = virtualDataSource;
            var readWriteConnectors = virtualDataSource.ConfigurationParams.ReadWriteNodeSeparationConfigs.Select(o=> readWriteConnectorFactory.CreateConnector(virtualDataSource.ConfigurationParams.ReadStrategy.GetValueOrDefault(), o.Key,o.Value));
            _shardingConnectionStringResolver = new ReadWriteShardingConnectionStringResolver(readWriteConnectors, virtualDataSource.ConfigurationParams.ReadStrategy.GetValueOrDefault(),readWriteConnectorFactory);
        }
        public string GetConnectionString(string dataSourceName)
        {
            return GetReadNodeConnectionString(dataSourceName,null);
           
        }

        public string GetReadNodeConnectionString(string dataSourceName, string readNodeName)
        {
            if (!_shardingConnectionStringResolver.ContainsReadWriteDataSourceName(dataSourceName))
                return _virtualDataSource.GetConnectionString(dataSourceName);
            return _shardingConnectionStringResolver.GetConnectionString(dataSourceName, readNodeName);
        }

        public bool AddReadConnectionString(string dataSourceName, string connectionString, string readNodeName)
        {
            return _shardingConnectionStringResolver.AddConnectionString(dataSourceName, connectionString, readNodeName);
        }
    }
}
