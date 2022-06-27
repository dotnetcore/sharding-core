using ShardingCore.Exceptions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    public class ReadWriteShardingConnectionStringResolver : IShardingConnectionStringResolver
    {
        private readonly ReadStrategyEnum _readStrategy;

        private readonly ConcurrentDictionary<string, IReadWriteConnector> _connectors =
            new ConcurrentDictionary<string, IReadWriteConnector>();

        private readonly IReadWriteConnectorFactory _readWriteConnectorFactory;
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        public ReadWriteShardingConnectionStringResolver(IEnumerable<IReadWriteConnector> connectors, ReadStrategyEnum readStrategy,IReadWriteConnectorFactory readWriteConnectorFactory)
        {
            _readStrategy = readStrategy;
            var enumerator = connectors.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentConnector = enumerator.Current;
                if (currentConnector != null)
                    _connectors.TryAdd(currentConnector.DataSourceName, currentConnector);
            }

            _readWriteConnectorFactory = readWriteConnectorFactory;
        }

        public bool ContainsReadWriteDataSourceName(string dataSourceName)
        {
            return _connectors.ContainsKey(dataSourceName);
        }

        public string GetConnectionString(string dataSourceName, string readNodeName)
        {
            if (!_connectors.TryGetValue(dataSourceName, out var connector))
                throw new ShardingCoreInvalidOperationException($"read write connector not found, data source name:[{dataSourceName}]");
            return connector.GetConnectionString(readNodeName);
        }
        public bool AddConnectionString(string dataSourceName, string connectionString, string readNodeName)
        {
            if (!_connectors.TryGetValue(dataSourceName, out var connector))
            {
                connector = _readWriteConnectorFactory.CreateConnector(_readStrategy,
                    dataSourceName, new ReadNode[]
                    {
                        new ReadNode(readNodeName??Guid.NewGuid().ToString("n"),connectionString)
                    });
                return _connectors.TryAdd(dataSourceName, connector);
            }
            else
            {
                return connector.AddConnectionString(connectionString, readNodeName);
            }
        }
    }
}
