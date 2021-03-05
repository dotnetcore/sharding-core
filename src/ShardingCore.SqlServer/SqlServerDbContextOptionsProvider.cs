using System.Collections.Generic;
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
    public class SqlServerDbContextOptionsProvider:IDbContextOptionsProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IShardingCoreOptions _shardingCoreOptions;

        private readonly Dictionary<string, ShareDbContextWrapItem> _contextWrapItems =
            new Dictionary<string, ShareDbContextWrapItem>();

        public SqlServerDbContextOptionsProvider(ILoggerFactory loggerFactory,IShardingCoreOptions shardingCoreOptions)
        {
            _loggerFactory = loggerFactory;
            _shardingCoreOptions = shardingCoreOptions;
        }
        public DbContextOptions GetDbContextOptions(string connectKey)
        {
            if (_contextWrapItems.ContainsKey(connectKey))
            {
                var connectionString = _shardingCoreOptions.GetShardingConfig(connectKey).ConnectionString;
                var connection = new SqlConnection(connectionString);
                var dbContextOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(connection)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .UseLoggerFactory(_loggerFactory)
                    .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                    .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                    .UseShardingSqlServerQuerySqlGenerator()
                    .Options;
                _contextWrapItems.Add(connectKey, new ShareDbContextWrapItem(connection, dbContextOptions));
            }
            return _contextWrapItems[connectKey].ContextOptions;
        }

        public void Dispose()
        {
            _contextWrapItems.ForEach(o => o.Value.Connection?.Dispose());
        }
    }
}