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
    * @Date: 2021/9/19 21:33:21
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDefaultDataSourceBuilder<TShardingDbContext, TActualDbContext>: ShardingCoreConfigEndBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> _shardingCoreConfigBuilder;

        public ShardingDefaultDataSourceBuilder(ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> shardingCoreConfigBuilder):base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }
        public ShardingDataBaseOrTableBuilder<TShardingDbContext, TActualDbContext> AddDefaultDataSource(string dataSourceName, string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(_shardingCoreConfigBuilder.ShardingConfigOption.DefaultDataSourceName) || !string.IsNullOrWhiteSpace(_shardingCoreConfigBuilder.ShardingConfigOption.DefaultConnectionString))
                throw new InvalidOperationException($"{nameof(AddDefaultDataSource)}-{dataSourceName}");
            _shardingCoreConfigBuilder.ShardingConfigOption.DefaultDataSourceName = dataSourceName;
            _shardingCoreConfigBuilder.ShardingConfigOption.DefaultConnectionString = connectionString;
            return new ShardingDataBaseOrTableBuilder<TShardingDbContext, TActualDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
