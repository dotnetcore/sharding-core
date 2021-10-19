//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Sharding.Abstractions;

//namespace ShardingCore.DIExtensions
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/19 21:13:43
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public class ShardingTransactionBuilder<TShardingDbContext>
//        where TShardingDbContext : DbContext, IShardingDbContext
//    {
//        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

//        public ShardingTransactionBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder)
//        {
//            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
//        }
//        public ShardingDefaultDataSourceBuilder<TShardingDbContext> AddShardingTransaction(Action<DbConnection, DbContextOptionsBuilder> transactionConfigure)
//        {
//            _shardingCoreConfigBuilder.ShardingConfigOption.UseShardingTransaction(transactionConfigure);
//            return new ShardingDefaultDataSourceBuilder<TShardingDbContext>(_shardingCoreConfigBuilder);
//        }
//    }
//}
