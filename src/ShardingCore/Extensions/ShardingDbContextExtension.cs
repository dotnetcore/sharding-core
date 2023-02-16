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
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.EFCores;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Extensions
{
    public static class ShardingDbContextExtension
    {
        public static bool IsShellDbContext(this DbContext dbContext)
        {
            return dbContext.GetService<IDbContextOptions>().FindExtension<ShardingWrapOptionsExtension>()!=null;
        }
        public static IShardingDbContextExecutor CreateShardingDbContextExecutor<TDbContext>(
            this TDbContext shellDbContext)
        where TDbContext:DbContext,IShardingDbContext
        {
            if (shellDbContext.IsShellDbContext())
            {
                return new ShardingDbContextExecutor(shellDbContext);
            }

            return default;
        }
        public static bool IsUseReadWriteSeparation(this IShardingDbContext shardingDbContext)
        {
            return shardingDbContext.GetShardingExecutor().GetVirtualDataSource().UseReadWriteSeparation;
        }

        public static bool SupportUnionAllMerge(this IShardingDbContext shardingDbContext)
        {
            var dbContext = (DbContext)shardingDbContext;
            return dbContext.GetService<IDbContextServices>().ContextOptions.FindExtension<UnionAllMergeOptionsExtension>() is not null;
        }

        /// <summary>
        /// 创建共享链接DbConnection
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        public static DbContext GetShareDbContext(this IShardingDbContext shardingDbContext,string dataSourceName,IRouteTail routeTail)
        {
            return shardingDbContext.GetShardingExecutor().CreateDbContext(CreateDbContextStrategyEnum.ShareConnection,dataSourceName, routeTail);
        }
        
        /// <summary>
        /// 获取独立生命周期的写连接字符串的db context
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        public static DbContext GetIndependentWriteDbContext(this IShardingDbContext shardingDbContext,string dataSourceName,IRouteTail routeTail)
        {
            return shardingDbContext.GetShardingExecutor().CreateDbContext(CreateDbContextStrategyEnum.IndependentConnectionWrite,dataSourceName, routeTail);
        }
        /// <summary>
        /// 获取独立生命周期的读连接字符串的db context
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        public static DbContext GetIndependentQueryDbContext(this IShardingDbContext shardingDbContext,string dataSourceName,IRouteTail routeTail)
        {
            return shardingDbContext.GetShardingExecutor().CreateDbContext(CreateDbContextStrategyEnum.IndependentConnectionQuery,dataSourceName, routeTail);
        }
    }
}
