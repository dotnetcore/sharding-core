using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

#if !EFCORE5
using MySql.Data.MySqlClient;
#endif
#if EFCORE5
using MySqlConnector;
#endif
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.EFCores;

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
        private DbContextOptions _dbContextOptions;
        private MySqlConnection _connection;

        public MySqlDbContextOptionsProvider(MySqlOptions mySqlOptions,ILoggerFactory loggerFactory)
        {
            _connection=new MySqlConnection(mySqlOptions.ConnectionString);
            _dbContextOptions = new DbContextOptionsBuilder()
#if EFCORE5
                .UseMySql(_connection,mySqlOptions.ServerVersion,mySqlOptions.MySqlOptionsAction)
#endif
#if !EFCORE5
                .UseMySql(_connection,mySqlOptions.MySqlOptionsAction)
#endif
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseLoggerFactory(loggerFactory)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .UseShardingSqlServerQuerySqlGenerator()
                .Options;
        }
        public DbContextOptions GetDbContextOptions()
        {
            return _dbContextOptions;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}