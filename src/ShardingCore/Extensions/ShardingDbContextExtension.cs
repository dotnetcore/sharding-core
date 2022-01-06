using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    public static class ShardingDbContextExtension
    {
        public static bool IsUseReadWriteSeparation(this IShardingDbContext shardingDbContext)
        {
            return shardingDbContext.GetVirtualDataSource().UseReadWriteSeparation;
        }
    }
}
