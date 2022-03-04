using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
    public static class ShardingDbContextExtension
    {
        public static bool IsUseReadWriteSeparation(this IShardingDbContext shardingDbContext)
        {
            return shardingDbContext.GetVirtualDataSource().UseReadWriteSeparation;
        }

        public static bool SupportUnionAllMerge(this IShardingDbContext shardingDbContext)
        {
            var dbContext = (DbContext)shardingDbContext;
            return dbContext.GetService<IDbContextServices>().ContextOptions.FindExtension<UnionAllMergeOptionsExtension>() is not null;
        }
    }
}
