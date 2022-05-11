using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Abstractions
{
    public interface IReadWriteConnectionStringManager
    {
        string GetReadNodeConnectionString(string dataSourceName,string readNodeName);
        bool AddReadConnectionString(string dataSourceName,string connectionString, string readNodeName);
    }
}
