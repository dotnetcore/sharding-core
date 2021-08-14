using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 09:57:08
* @Email: 326308290@qq.com
*/
/// <summary>
/// 分表分库的dbcontext
/// </summary>
/// <typeparam name="T"></typeparam>
    public class ShardingDbContext<T>:DbContext,IShardingDbContext where T:DbContext
{
    private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        public ShardingDbContext()
        {
            _shardingParallelDbContextFactory = ShardingContainer.GetService<IShardingParallelDbContextFactory>();
        }
        public DbContext GetDbContext(string tail)
        {
            return _shardingParallelDbContextFactory.Create(tail);
        }
    }
}