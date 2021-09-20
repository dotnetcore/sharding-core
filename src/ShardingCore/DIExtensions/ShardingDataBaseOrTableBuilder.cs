using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 21:37:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDataBaseOrTableBuilder<TShardingDbContext, TActualDbContext>: ShardingReadWriteSeparationBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> _shardingCoreConfigBuilder;

        public ShardingDataBaseOrTableBuilder(ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> shardingCoreConfigBuilder):base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingTableBuilder<TShardingDbContext, TActualDbContext> AddShardingDataBase(Func<IServiceProvider, IDictionary<string, string>> dataSourcesConfigure, Action<ShardingDatabaseOptions> shardingDatabaseConfigure)
        {
           
            _shardingCoreConfigBuilder.ShardingConfigOption.AddShardingDataSource(dataSourcesConfigure);
            var shardingDatabaseOptions = new ShardingDatabaseOptions();
            shardingDatabaseConfigure.Invoke(shardingDatabaseOptions);
            var shardingDatabaseRoutes = shardingDatabaseOptions.GetShardingDatabaseRoutes();
            foreach (var shardingDatabaseRoute in shardingDatabaseRoutes)
            {
                _shardingCoreConfigBuilder.ShardingConfigOption.AddShardingDataSourceRoute(shardingDatabaseRoute);
            }

            return new ShardingTableBuilder<TShardingDbContext, TActualDbContext>(_shardingCoreConfigBuilder);
        }

        public ShardingReadWriteSeparationBuilder<TShardingDbContext, TActualDbContext> AddShardingTable(Action<ShardingTableOptions> shardingTableConfigure)
        {

            var shardingTableOptions = new ShardingTableOptions();
            shardingTableConfigure.Invoke(shardingTableOptions);
            var shardingTableRoutes = shardingTableOptions.GetShardingTableRoutes();
            foreach (var shardingTableRoute in shardingTableRoutes)
            {
                _shardingCoreConfigBuilder.ShardingConfigOption.AddShardingTableRoute(shardingTableRoute);
            }
            return new ShardingReadWriteSeparationBuilder<TShardingDbContext, TActualDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
