using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.TableExists.Abstractions;

namespace ShardingCore.TableExists
{
    public class EmptyTableEnsureManager : ITableEnsureManager
    {
        public ISet<string> GetExistTables(IShardingDbContext shardingDbContext, string dataSourceName)
        {
            return new HashSet<string>();
        }
    }
}
