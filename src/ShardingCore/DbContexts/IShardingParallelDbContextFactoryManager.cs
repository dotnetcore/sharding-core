using System;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 20 February 2021 15:04:25
* @Email: 326308290@qq.com
*/
    public interface IShardingParallelDbContextFactoryManager
    {
        public DbContext CreateDbContext(string connectKey, string tail);
    }
}