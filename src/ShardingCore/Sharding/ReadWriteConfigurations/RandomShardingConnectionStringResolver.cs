using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 14:22:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class RandomShardingConnectionStringResolver<TShardingDbContext> : IShardingConnectionStringResolver<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ConcurrentDictionary<string, ReadWriteRandomConnector> _connectors =
            new ConcurrentDictionary<string, ReadWriteRandomConnector>();
        public RandomShardingConnectionStringResolver(IEnumerable<ReadWriteRandomConnector> connectors)
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
