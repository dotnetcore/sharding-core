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
    * @Date: 2021/9/19 22:56:44
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingTableBuilder<TShardingDbContext> : ShardingReadWriteSeparationBuilder<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly ShardingCoreConfigBuilder<TShardingDbContext> _shardingCoreConfigBuilder;

        public ShardingTableBuilder(ShardingCoreConfigBuilder<TShardingDbContext> shardingCoreConfigBuilder) : base(shardingCoreConfigBuilder)
        {
            _shardingCoreConfigBuilder = shardingCoreConfigBuilder;
        }

        public ShardingReadWriteSeparationBuilder<TShardingDbContext> AddShardingTableRoute(Action<ShardingTableOptions> shardingTableConfigure)
        {

            var shardingTableOptions = new ShardingTableOptions();
            shardingTableConfigure.Invoke(shardingTableOptions);
            var shardingTableRoutes = shardingTableOptions.GetShardingTableRoutes();
            foreach (var shardingTableRoute in shardingTableRoutes)
            {
                _shardingCoreConfigBuilder.ShardingConfigOption.AddShardingTableRoute(shardingTableRoute);
            }
            return new ShardingReadWriteSeparationBuilder<TShardingDbContext>(_shardingCoreConfigBuilder);
        }
        public ShardingTableBuilder<TShardingDbContext>  ReplaceShardingComparer(Func<IServiceProvider, IShardingComparer<TShardingDbContext>> newShardingComparerFactory)
        {
            _shardingCoreConfigBuilder.ShardingConfigOption.ReplaceShardingComparer(newShardingComparerFactory);
            return this;
        }
    }
}
