using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ParallelTables
{
    public interface IParallelTableManager<TShardingDbContext> : IParallelTableManager
        where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
