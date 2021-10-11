using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 21:11:48
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingQueryBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingQueryBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }
        public ShardingTransactionBuilder<TShardingDbContext> AddShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.UseShardingQuery(queryConfigure);
            return new ShardingTransactionBuilder<TShardingDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
