using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
#if !EFCORE5
using MySql.Data.MySqlClient;
#endif
#if EFCORE5
using MySqlConnector;
#endif
using ShardingCore.DbContexts.VirtualDbContexts;
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
        
        public MySqlDbContextOptionsProvider(MySqlOptions mySqlOptions,ILoggerFactory loggerFactory,IShardingCoreOptions shardingCoreOptions)
        {
            _mySqlOptions = mySqlOptions;
            _loggerFactory = loggerFactory;
            _shardingCoreOptions = shardingCoreOptions;
        }
        public DbContextOptions GetDbContextOptions(DbConnection dbConnection)
        {

            var track = dbConnection != null;
            var connection = dbConnection ?? GetMySqlConnection();
            var dbContextOptions= CreateDbContextOptionBuilder()
#if EFCORE5
                .UseMySql(connection, _mySqlOptions.ServerVersion, _mySqlOptions.MySqlOptionsAction)
#endif
#if !EFCORE5
                    .UseMySql(connection, _mySqlOptions.MySqlOptionsAction)
#endif
                .IfDo(!track, o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
                .UseLoggerFactory(_loggerFactory)
                //.ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer>()
                //.UseShardingMySqlQuerySqlGenerator()
                .Options;
            return dbContextOptions;
        }
        private MySqlConnection GetMySqlConnection()
        {
            var connectionString = _shardingCoreOptions.GetShardingConfig().ConnectionString;
            return  new MySqlConnection(connectionString);
        }
        private DbContextOptionsBuilder CreateDbContextOptionBuilder()
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig();
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(shardingConfigEntry.DbContextType);
            return  (DbContextOptionsBuilder)Activator.CreateInstance(type);
        }
        
    }
}