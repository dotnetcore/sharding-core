using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.ShardingTableDbContexts;

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
        public ShardingTableDbContext Create(ShardingTableDbContextOptions shardingDbContextOptions)
        {
            return new ShardingDbContext(shardingDbContextOptions);
        }
    }
}