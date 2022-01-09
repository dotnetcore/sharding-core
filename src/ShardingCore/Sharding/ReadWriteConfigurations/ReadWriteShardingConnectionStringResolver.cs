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
        public ReadWriteShardingConnectionStringResolver(IEnumerable<IReadWriteConnector> connectors, ReadStrategyEnum readStrategy)
        {
            _readStrategy = readStrategy;
            var enumerator = connectors.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentConnector = enumerator.Current;
                if (currentConnector != null)
                    _connectors.TryAdd(currentConnector.DataSourceName, currentConnector);
            }

            _readWriteConnectorFactory = ShardingContainer.GetService<IReadWriteConnectorFactory>();
        }

        public bool ContainsReadWriteDataSourceName(string dataSourceName)
        {
            return _connectors.ContainsKey(dataSourceName);
        }

        public string GetConnectionString(string dataSourceName)
        {
            if (!_connectors.TryGetValue(dataSourceName, out var connector))
                throw new ShardingCoreInvalidOperationException($"read write connector not found, data source name:[{dataSourceName}]");
            return connector.GetConnectionString();
        }

        public bool AddConnectionString(string dataSourceName, string connectionString)
        {
            if (!_connectors.TryGetValue(dataSourceName, out var connector))
            {
                connector = _readWriteConnectorFactory.CreateConnector(_readStrategy,
                    dataSourceName, new List<string>()
                    {
                        connectionString
                    });
                return _connectors.TryAdd(dataSourceName, connector);
            }
            else
            {
                return connector.AddConnectionString(connectionString);
            }
        }
    }
}
