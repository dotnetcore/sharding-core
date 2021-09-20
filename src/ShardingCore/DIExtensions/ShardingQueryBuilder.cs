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
    public class ShardingQueryBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> _shardingCoreConfigBuilder;

        public ShardingQueryBuilder(ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }
        public ShardingTransactionBuilder<TShardingDbContext, TActualDbContext> AddShardingQuery(Action<string, DbContextOptionsBuilder> queryConfigure)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.UseShardingQuery(queryConfigure);
            return new ShardingTransactionBuilder<TShardingDbContext, TActualDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
