using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.Core.VirtualTables;
#if !EFCORE5
using MySql.Data.MySqlClient;
#endif
#if EFCORE5
using MySqlConnector;
#endif
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts.ShareDbContextOptionsProviders;
using ShardingCore.EFCores;
using ShardingCore.Extensions;

namespace ShardingCore.MySql
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 10:33:51
* @Email: 326308290@qq.com
*/
    public class MySqlDbContextOptionsProvider:IDbContextOptionsProvider
    {
        private readonly MySqlOptions _mySqlOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IShardingCoreOptions _shardingCoreOptions;

        private readonly Dictionary<string, ShareDbContextWrapItem> _contextWrapItems =
            new Dictionary<string, ShareDbContextWrapItem>();

        public MySqlDbContextOptionsProvider(MySqlOptions mySqlOptions,ILoggerFactory loggerFactory,IShardingCoreOptions shardingCoreOptions)
        {
            _mySqlOptions = mySqlOptions;
            _loggerFactory = loggerFactory;
            _shardingCoreOptions = shardingCoreOptions;
        }
        public DbContextOptions GetDbContextOptions()
        {
            var connectionString = _shardingCoreOptions.GetShardingConfig().ConnectionString;
            var connection = new MySqlConnection(connectionString);
            var dbContextOptions= CreateDbContextOptionBuilder()
#if EFCORE5
                .UseMySql(connection, _mySqlOptions.ServerVersion, _mySqlOptions.MySqlOptionsAction)
#endif
#if !EFCORE5
                    .UseMySql(connection, _mySqlOptions.MySqlOptionsAction)
#endif
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseLoggerFactory(_loggerFactory)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer>()
                .UseShardingMySqlQuerySqlGenerator()
                .Options;
            return dbContextOptions;
        }
        private DbContextOptionsBuilder CreateDbContextOptionBuilder()
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig();
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(shardingConfigEntry.DbContextType);
            return  (DbContextOptionsBuilder)Activator.CreateInstance(type);
        }

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        public void Dispose()
        {
            _contextWrapItems.ForEach(o=>o.Value.Connection?.Dispose());
        }
    }
}