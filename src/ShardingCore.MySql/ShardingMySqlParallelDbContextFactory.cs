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
        private readonly MySqlOptions _mySqlOptions;

        public ShardingMySqlParallelDbContextFactory(IVirtualTableManager virtualTableManager, MySqlOptions mySqlOptions)
        {
            _virtualTableManager = virtualTableManager;
            _mySqlOptions = mySqlOptions;
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
#if EFCORE5
                .UseMySql(_mySqlOptions.ConnectionString,_mySqlOptions.ServerVersion,_mySqlOptions.MySqlOptionsAction)
#endif
#if !EFCORE5
            
                .UseMySql(_mySqlOptions.ConnectionString,_mySqlOptions.MySqlOptionsAction)
#endif
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .UseShardingSqlServerQuerySqlGenerator()
                .Options;
        }
    }
}