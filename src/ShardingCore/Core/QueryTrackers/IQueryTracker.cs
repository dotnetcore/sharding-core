using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.QueryTrackers
{
    public interface IQueryTracker
    {
        public object Track(object entity,IShardingDbContext shardingDbContext);
    }
}
