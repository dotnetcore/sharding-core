using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    public interface IReadWriteConnectorFactory
    {
        IReadWriteConnector CreateConnector(ReadStrategyEnum strategy, string dataSourceName,
            ReadNode[] readNodes);
    }
}
