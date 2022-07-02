using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.TableExists.Abstractions
{
    public interface ITableEnsureManager
    {
        ISet<string> GetExistTables(IShardingDbContext shardingDbContext, string dataSourceName);
    }
}
