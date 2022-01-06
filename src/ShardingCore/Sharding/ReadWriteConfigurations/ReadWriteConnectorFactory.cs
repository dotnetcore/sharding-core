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
        public IReadWriteConnector CreateConnector(ReadStrategyEnum strategy,string dataSourceName, IEnumerable<string> connectionStrings)
        {

            if (strategy == ReadStrategyEnum.Loop)
            {
                return new ReadWriteLoopConnector(dataSourceName, connectionStrings);
            }
            else if (strategy == ReadStrategyEnum.Random)
            {
                return new ReadWriteRandomConnector(dataSourceName, connectionStrings);
            }
            else
            {
                throw new ShardingCoreInvalidOperationException(
                    $"unknown read write strategy:[{strategy}]");
            }
        }
    }
}
