using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.DbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 24 December 2020 08:22:48
    * @Email: 326308290@qq.com
    */
    public class ShardingDbContextFactory<TShardingDbContext> : IShardingDbContextFactory<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingDbContextCreatorConfig _shardingDbContextCreatorConfig;

        public ShardingDbContextFactory(IEnumerable<IShardingDbContextCreatorConfig> shardingDbContextCreatorConfigs)
        {
            _shardingDbContextCreatorConfig = shardingDbContextCreatorConfigs
                .FirstOrDefault(o => o.ShardingDbContextType == typeof(TShardingDbContext))
                ??throw new ShardingCoreException(
                $"{typeof(TShardingDbContext).FullName} cant found DefaultShardingDbContextCreatorConfig<{typeof(TShardingDbContext).Name}> should use {nameof(DIExtension.AddShardingDbContext)}");
        }
        public DbContext Create(ShardingDbContextOptions shardingDbContextOptions)
        {
            var routeTail=shardingDbContextOptions.RouteTail;
            
             var dbContext = _shardingDbContextCreatorConfig.Creator(shardingDbContextOptions);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = routeTail;
            }

            //if (dbContext is IShardingDbContext shardingDbContext)
            //{
            //    shardingDbContext.ShardingUpgrade();
            //}
            //else
            //{
            //    throw new ShardingCoreException($"{dbContext.GetType().FullName} should implements {nameof(IShardingDbContext)}");
            //}
            var dbContextModel = dbContext.Model;
            return dbContext;
        }
    }
}