using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 08:22:48
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextFactory:IShardingDbContextFactory
    {
        private readonly IShardingCoreOptions _shardingCoreOptions;

        public ShardingDbContextFactory(IShardingCoreOptions shardingCoreOptions)
        {
            _shardingCoreOptions = shardingCoreOptions;
        }
        public DbContext Create(string connectKey, ShardingDbContextOptions shardingDbContextOptions)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            return shardingConfigEntry.Creator(shardingDbContextOptions);
        }
    }
}