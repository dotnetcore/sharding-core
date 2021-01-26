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
        public ShardingDbContext Create(ShardingDbContextOptions shardingDbContextOptions)
        {
            return new ShardingDbContext(shardingDbContextOptions);
        }
    }
}