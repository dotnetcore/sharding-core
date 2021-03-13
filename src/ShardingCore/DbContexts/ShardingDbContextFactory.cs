using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

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
        private readonly IShardingCoreOptions _shardingCoreOptions;
        private readonly IShardingTableScopeFactory _shardingTableScopeFactory;
        private readonly IDbContextCreateFilterManager _dbContextCreateFilterManager;

        public ShardingDbContextFactory(IShardingCoreOptions shardingCoreOptions,IShardingTableScopeFactory shardingTableScopeFactory, IDbContextCreateFilterManager dbContextCreateFilterManager)
        {
            _shardingCoreOptions = shardingCoreOptions;
            _shardingTableScopeFactory = shardingTableScopeFactory;
            _dbContextCreateFilterManager = dbContextCreateFilterManager;
        }
        public DbContext Create(string connectKey, ShardingDbContextOptions shardingDbContextOptions,IServiceProvider serviceProvider)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            
            using (var scope = _shardingTableScopeFactory.CreateScope())
            {
                string tail = null;
                string modelChangeKey = null;
                if (!string.IsNullOrWhiteSpace(shardingDbContextOptions.Tail))
                {
                    tail = shardingDbContextOptions.Tail;
                    modelChangeKey = $"sharding_{tail}";
                }
                scope.ShardingTableAccessor.Context = ShardingTableContext.Create(connectKey,tail);
                var dbContext=  shardingConfigEntry.Creator(shardingDbContextOptions);
                if (modelChangeKey != null&& dbContext is IShardingTableDbContext shardingTableDbContext)
                {
                    shardingTableDbContext.ModelChangeKey = modelChangeKey;
                }

                if (serviceProvider != null)
                {
                    foreach (var dbContextCreateFilter in _dbContextCreateFilterManager.GetFilters())
                    {
                        dbContextCreateFilter.CreateAfter(dbContext, serviceProvider);
                    }
                }
                var dbContextModel = dbContext.Model;
                return dbContext;
            }
        }

        public DbContext Create(string connectKey, string tail,IServiceProvider serviceProvider)
        {
            var dbContextOptionsProvider = serviceProvider.GetService<IDbContextOptionsProvider>();
            var shardingDbContextOptions =
                new ShardingDbContextOptions(dbContextOptionsProvider.GetDbContextOptions(connectKey), tail);
           return Create(connectKey,shardingDbContextOptions,serviceProvider);
        }
    }
}