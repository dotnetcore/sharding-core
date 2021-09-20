using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;

namespace ShardingCore.DIExtensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/19 22:44:54
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingReadWriteSeparationBuilder<TShardingDbContext, TActualDbContext> : ShardingCoreConfigEndBuilder<TShardingDbContext, TActualDbContext>
        where TActualDbContext : DbContext
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> _shardingCoreConfigBuilder;

        public ShardingReadWriteSeparationBuilder(ShardingCoreConfigBuilder<TShardingDbContext, TActualDbContext> shardingCoreConfigBuilder) : base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingCoreConfigEndBuilder<TShardingDbContext, TActualDbContext> AddReadWriteSeparation(
            Func<IServiceProvider, IDictionary<string, ISet<string>>> readWriteSeparationConfigure,
            ReadStrategyEnum readStrategyEnum,
            bool defaultEnable = false,
            int defaultPriority = 10,
            ReadConnStringGetStrategyEnum readConnStringGetStrategy = ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.UseReadWriteConfiguration(readWriteSeparationConfigure,readStrategyEnum, defaultEnable,defaultPriority);
            return new ShardingCoreConfigEndBuilder<TShardingDbContext, TActualDbContext>(_shardingCoreConfigBuilder);
        }
    }
}
