using System;
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
    public class ReadWriteConnectorFactory: IReadWriteConnectorFactory
    {
        public IReadWriteConnector CreateConnector<TShardingDbContext>(ReadStrategyEnum strategy,string dataSourceName, IEnumerable<string> connectionStrings) where TShardingDbContext : DbContext, IShardingDbContext
        {
            var readWriteOptions = ShardingContainer.GetService<IReadWriteOptions<TShardingDbContext>>();
            if (readWriteOptions == null)
                throw new ShardingCoreInvalidOperationException(
                    "cant create read write connector should use read write");

            if (readWriteOptions.ReadStrategy == ReadStrategyEnum.Loop)
            {
                return new ReadWriteLoopConnector(dataSourceName, connectionStrings);
            }
            else if (readWriteOptions.ReadStrategy == ReadStrategyEnum.Random)
            {
                return new ReadWriteRandomConnector(dataSourceName, connectionStrings);
            }
            else
            {
                throw new ShardingCoreInvalidOperationException(
                    $"unknown read write strategy:[{readWriteOptions.ReadStrategy}]");
            }
        }
    }
}
