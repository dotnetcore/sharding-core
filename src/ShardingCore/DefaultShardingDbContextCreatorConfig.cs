using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using System;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 17:30:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultShardingDbContextCreatorConfig<TShardingDbContext> : IShardingDbContextCreatorConfig<TShardingDbContext>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Func<ShardingDbContextOptions, DbContext> _creator;
        public DefaultShardingDbContextCreatorConfig()
        {
            _creator = ShardingCoreHelper.CreateActivator<TShardingDbContext>();
        }

        public Type ShardingDbContextType => typeof(TShardingDbContext);
        public DbContext Creator(ShardingDbContextOptions shardingDbContextOptions)
        {
            return _creator(shardingDbContextOptions);
        }
    }
}
