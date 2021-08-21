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
    public class ShardingDbContextFactory:IShardingDbContextFactory
    {
        private readonly IEnumerable<IShardingDbContextCreatorConfig> _shardingDbContextCreatorConfigs;

        public ShardingDbContextFactory(IEnumerable<IShardingDbContextCreatorConfig> shardingDbContextCreatorConfigs)
        {
            _shardingDbContextCreatorConfigs = shardingDbContextCreatorConfigs;
        }
        public DbContext Create(Type shardingDbContextType, ShardingDbContextOptions shardingDbContextOptions)
        {
            if (!shardingDbContextType.IsShardingDbContext())
                throw new ShardingCoreException(
                    $"{shardingDbContextType.FullName} must impl {nameof(IShardingDbContext)}");
            var shardingDbContextCreatorConfig = _shardingDbContextCreatorConfigs.FirstOrDefault(o=>o.ShardingDbContextType==shardingDbContextType);
            if (shardingDbContextCreatorConfig == null)
            {
                throw new ShardingCoreException(
                    $"{shardingDbContextType.FullName} cant found DefaultShardingDbContextCreatorConfig<{shardingDbContextType.Name}> should use {nameof(DIExtension.AddShardingDbContext)}");
            }
            var tail=shardingDbContextOptions.Tail;
            
             var dbContext = shardingDbContextCreatorConfig.Creator(shardingDbContextOptions);
            if (!string.IsNullOrWhiteSpace(tail) && dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.SetShardingTableDbContextTail(tail);
            }
            var dbContextModel = dbContext.Model;
            return dbContext;
        }


        public DbContext Create<TShardingDbContext>(ShardingDbContextOptions shardingDbContextOptions) where TShardingDbContext : DbContext, IShardingDbContext
        {
            return Create(typeof(TShardingDbContext), shardingDbContextOptions);
        }
    }
}