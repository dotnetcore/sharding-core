using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Sharding.Abstractions
{
    public interface ICurrentDbContextDiscover
    {
        IDictionary<string, IDataSourceDbContext> GetCurrentDbContexts();
    }
}
