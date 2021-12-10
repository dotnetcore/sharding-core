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
    public class ReadWriteShardingConnectionStringResolver<TShardingDbContext> : IShardingConnectionStringResolver<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ConcurrentDictionary<string, IReadWriteConnector> _connectors =
            new ConcurrentDictionary<string, IReadWriteConnector>();

        private readonly IReadWriteOptions<TShardingDbContext> _readWriteOptions;
        public ReadWriteShardingConnectionStringResolver(IEnumerable<IReadWriteConnector> connectors)
        {
            var enumerator = connectors.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentConnector = enumerator.Current;
                if (currentConnector != null)
                    _connectors.TryAdd(currentConnector.DataSourceName, currentConnector);
            }

            _readWriteOptions = ShardingContainer.GetService<IReadWriteOptions<TShardingDbContext>>();
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
                if (_readWriteOptions.ReadStrategy == ReadStrategyEnum.Loop)
                {
                    connector= new ReadWriteLoopConnector(dataSourceName, new List<string> { connectionString });
                }
                else if (_readWriteOptions.ReadStrategy == ReadStrategyEnum.Random)
                {
                    connector= new ReadWriteLoopConnector(dataSourceName, new List<string> { connectionString });
                }

                throw new ShardingCoreInvalidOperationException(
                    $"unknown read write strategy:[{_readWriteOptions.ReadStrategy}]");

            }
            else
            {
                return connector.AddConnectionString(connectionString);
            }
        }
    }
}
