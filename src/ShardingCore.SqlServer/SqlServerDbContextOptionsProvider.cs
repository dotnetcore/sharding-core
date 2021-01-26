using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.EFCores;
#if EFCORE2
using System.Data.SqlClient;
#endif
#if !EFCORE2
using Microsoft.Data.SqlClient;
#endif

namespace ShardingCore.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 10:33:51
* @Email: 326308290@qq.com
*/
    public class SqlServerDbContextOptionsProvider:IDbContextOptionsProvider
    {
        private DbContextOptions _dbContextOptions;
        private SqlConnection _connection;

        public SqlServerDbContextOptionsProvider(SqlServerOptions sqlServerOptions,ILoggerFactory loggerFactory)
        {
            _connection=new SqlConnection(sqlServerOptions.ConnectionString);
            _dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(_connection)
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