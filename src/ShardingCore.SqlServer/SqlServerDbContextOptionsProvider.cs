using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts.ShareDbContextOptionsProviders;
using ShardingCore.EFCores;
using ShardingCore.Extensions;
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
    public class SqlServerDbContextOptionsProvider : IDbContextOptionsProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IShardingCoreOptions _shardingCoreOptions;



        public SqlServerDbContextOptionsProvider(ILoggerFactory loggerFactory, IShardingCoreOptions shardingCoreOptions)
        {
            _loggerFactory = loggerFactory;
            _shardingCoreOptions = shardingCoreOptions;
        }
        public DbContextOptions GetDbContextOptions(DbConnection dbConnection)
        {
            Console.WriteLine("create new dbcontext options,dbconnection is new:"+(dbConnection==null));

            var track = dbConnection != null;
            var connection = dbConnection ?? GetSqlConnection();
            var dbContextOptions = CreateDbContextOptionBuilder()
                .UseSqlServer(connection)
                .UseLoggerFactory(_loggerFactory)
                .IfDo(!track, o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
                //.IfDo(isQuery,o=>o.ReplaceService<IQueryCompiler, ShardingQueryCompiler>())
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer>()
                //.IfDo(isQuery,o=>o.UseShardingSqlServerQuerySqlGenerator())
                .Options;
            return dbContextOptions;
        }
        private SqlConnection GetSqlConnection()
        {
            var connectionString = _shardingCoreOptions.GetShardingConfig().ConnectionString;
            return new SqlConnection(connectionString);
        }


        private DbContextOptionsBuilder CreateDbContextOptionBuilder()
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig();
            Type type = typeof(DbContextOptionsBuilder<>);
            type = type.MakeGenericType(shardingConfigEntry.DbContextType);
            return (DbContextOptionsBuilder)Activator.CreateInstance(type);
        }
    }
}