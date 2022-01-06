using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    public class ReadWriteShardingConnectionStringResolver : IShardingConnectionStringResolver
    {
        private readonly ReadStrategyEnum _readStrategy;

        private readonly ConcurrentDictionary<string, IReadWriteConnector> _connectors =
            new ConcurrentDictionary<string, IReadWriteConnector>();

        private readonly IReadWriteConnectorFactory _readWriteConnectorFactory;
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
                _connectors.TryAdd(dataSourceName, connector);
                return true;
            }
            else
            {
                return connector.AddConnectionString(connectionString);
            }
        }
    }
}
