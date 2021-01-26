using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.EFCores;
using ShardingCore.Extensions;

namespace ShardingCore.SqlServer
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 29 December 2020 15:22:50
* @Email: 326308290@qq.com
*/
    public class ShardingSqlServerParallelDbContextFactory : IShardingParallelDbContextFactory
    {
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly SqlServerOptions _sqlServerOptions;

        public ShardingSqlServerParallelDbContextFactory(IVirtualTableManager virtualTableManager, SqlServerOptions sqlServerOptions)
        {
            _virtualTableManager = virtualTableManager;
            _sqlServerOptions = sqlServerOptions;
        }

        public ShardingDbContext Create(string tail)
        {
            var virtualTableConfigs = _virtualTableManager.GetAllVirtualTables().GetVirtualTableDbContextConfigs();
            var shardingDbContextOptions = new ShardingDbContextOptions(CreateOptions(), tail, virtualTableConfigs);
            return new ShardingDbContext(shardingDbContextOptions);
        }

        private DbContextOptions CreateOptions()
        {
            return new DbContextOptionsBuilder()
                .UseSqlServer(_sqlServerOptions.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .UseShardingSqlServerQuerySqlGenerator()
                .Options;
        }
    }
}