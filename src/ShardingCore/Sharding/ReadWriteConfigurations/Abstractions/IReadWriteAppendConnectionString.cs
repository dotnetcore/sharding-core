using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    public interface IReadWriteAppendConnectionString
    {
        bool AddReadConnectionString(string dataSourceName,string connectionString);
    }
}
