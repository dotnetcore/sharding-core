using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 22:44:54
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteSeparationBuilder<TShardingDbContext> : ShardingCoreConfigEndBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingReadWriteSeparationBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder) : base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingCoreConfigEndBuilder<TShardingDbContext> AddReadWriteSeparation(
            Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> readWriteSeparationConfigure,
            ReadStrategyEnum readStrategyEnum,
            bool defaultEnable = false,
            int defaultPriority = 10,
            ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.UseReadWriteConfiguration(readWriteSeparationConfigure,readStrategyEnum, defaultEnable,defaultPriority, readConnStringGetStrategy);
            return new ShardingCoreConfigEndBuilder<TShardingDbContext>(_shardingCoreConfigBuilder);
        }
        public ShardingReadWriteSeparationBuilder<TShardingDbContext>  ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer<TShardingDbContext>> newShardingComparerFactory)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparer(newShardingComparerFactory);
            return this;
        }
    }
}
