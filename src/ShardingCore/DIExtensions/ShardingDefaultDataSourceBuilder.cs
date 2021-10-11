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
    public class ShardingDefaultDataSourceBuilder<TShardingDbContext>: ShardingCoreConfigEndBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingDefaultDataSourceBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder):base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }
        public ShardingDataBaseOrTableBuilder<TShardingDbContext> AddDefaultDataSource(string dataSourceName, string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(_shardingCoreConfigBuilder.ShardingConfigOption.DefaultDataSourceName) || !string.IsNullOrWhiteSpace(_shardingCoreConfigBuilder.ShardingConfigOption.DefaultConnectionString))
                throw new InvalidOperationException($"{nameof(AddDefaultDataSource)}-{dataSourceName}");
            _shardingCoreConfigBuilder.ShardingConfigOption.DefaultDataSourceName = dataSourceName;
            _shardingCoreConfigBuilder.ShardingConfigOption.DefaultConnectionString = connectionString;
            return new ShardingDataBaseOrTableBuilder<TShardingDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
