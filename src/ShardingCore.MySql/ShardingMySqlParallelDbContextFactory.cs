using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.EFCores;
using ShardingCore.Extensions;

namespace ShardingCore.MySql
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 29 December 2020 15:22:50
* @Email: 326308290@qq.com
*/
    public class ShardingMySqlParallelDbContextFactory : IShardingParallelDbContextFactory
    {
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly IShardingCoreOptions _shardingCoreOptions;
        private readonly MySqlOptions _mySqlOptions;

        public ShardingMySqlParallelDbContextFactory(IVirtualTableManager virtualTableManager,IShardingDbContextFactory shardingDbContextFactory,IShardingCoreOptions shardingCoreOptions, MySqlOptions mySqlOptions)
        {
            _virtualTableManager = virtualTableManager;
            _shardingDbContextFactory = shardingDbContextFactory;
            _shardingCoreOptions = shardingCoreOptions;
            _mySqlOptions = mySqlOptions;
        }

        public DbContext Create(string connectKey, string tail)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            var shardingDbContextOptions = new ShardingDbContextOptions(CreateOptions(connectKey,shardingConfigEntry.ConnectionString), tail);
            return _shardingDbContextFactory.Create(connectKey, shardingDbContextOptions);
        }

        private DbContextOptions CreateOptions(string connectKey, string connectionString)
        {
            return CreateDbContextOptionBuilder(connectKey)
#if EFCORE5
                .UseMySql(connectionString,_mySqlOptions.ServerVersion,_mySqlOptions.MySqlOptionsAction)
#endif
#if !EFCORE5
            
                .UseMySql(connectionString, _mySqlOptions.MySqlOptionsAction)
#endif
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer>()
                .UseShardingSqlServerQuerySqlGenerator()
                .Options;
        }
        private DbContextOptionsBuilder CreateDbContextOptionBuilder(string connectKey)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(shardingConfigEntry.DbContextType);
            return (DbContextOptionsBuilder)Activator.CreateInstance(type);
        }
    }
}