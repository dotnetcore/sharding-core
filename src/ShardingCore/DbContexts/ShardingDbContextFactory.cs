using System;
using System.Linq;
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
        private readonly IDbContextOptionsProvider _dbContextOptionsProvider;

        public ShardingDbContextFactory(IShardingCoreOptions shardingCoreOptions,IShardingTableScopeFactory shardingTableScopeFactory, IDbContextCreateFilterManager dbContextCreateFilterManager,IDbContextOptionsProvider dbContextOptionsProvider)
        {
            _shardingCoreOptions = shardingCoreOptions;
            _shardingTableScopeFactory = shardingTableScopeFactory;
            _dbContextCreateFilterManager = dbContextCreateFilterManager;
            _dbContextOptionsProvider = dbContextOptionsProvider;
        }
        public DbContext Create(ShardingDbContextOptions shardingDbContextOptions)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig();
            
            using (var scope = _shardingTableScopeFactory.CreateScope())
            {
                string tail = null;
                string modelChangeKey = null;
                if (!string.IsNullOrWhiteSpace(shardingDbContextOptions.Tail))
                {
                    tail = shardingDbContextOptions.Tail;
                    modelChangeKey = $"sharding_{tail}";
                }
                scope.ShardingTableAccessor.Context = ShardingTableContext.Create(tail);
                var dbContext=  shardingConfigEntry.Creator(shardingDbContextOptions);
                if (modelChangeKey != null&& dbContext is IShardingTableDbContext shardingTableDbContext)
                {
                    shardingTableDbContext.ModelChangeKey = modelChangeKey;
                }

                var filters = _dbContextCreateFilterManager.GetFilters();
                if (filters.Any())
                {
                    foreach (var dbContextCreateFilter in filters)
                    {
                        dbContextCreateFilter.CreateAfter(dbContext);
                    }
                }
                var dbContextModel = dbContext.Model;
                return dbContext;
            }
        }

        public DbContext Create(string tail, bool isQuery)
        {
            var shardingDbContextOptions =
                new ShardingDbContextOptions(_dbContextOptionsProvider.GetDbContextOptions(isQuery), tail);
           return Create(shardingDbContextOptions);
        }
    }
}