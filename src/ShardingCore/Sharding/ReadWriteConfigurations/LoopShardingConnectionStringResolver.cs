using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 14:39:23
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class LoopShardingConnectionStringResolver<TShardingDbContext> : IShardingConnectionStringResolver<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ConcurrentDictionary<string, ReadWriteLoopConnector> _connectors =
            new ConcurrentDictionary<string, ReadWriteLoopConnector>();
        public LoopShardingConnectionStringResolver(IEnumerable<ReadWriteLoopConnector> connectors)
        {
            var enumerator = connectors.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentConnector = enumerator.Current;
                if (currentConnector != null)
                    _connectors.TryAdd(currentConnector.DataSourceName, currentConnector);
            }
        }

        public string GetConnectionString(string dataSourceName)
        {
            if (!_connectors.TryGetValue(dataSourceName, out var connector))
                throw new ShardingCoreInvalidOperationException($"read write connector not found, data source name:[{dataSourceName}]");
            return connector.GetConnectionString();
        }
    }
}
