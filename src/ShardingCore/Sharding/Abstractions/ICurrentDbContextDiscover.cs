using System.Collections.Generic;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Sharding.Abstractions
{
    public interface ICurrentDbContextDiscover
    {
        IDictionary<string, IDataSourceDbContext> GetCurrentDbContexts();
    }
}