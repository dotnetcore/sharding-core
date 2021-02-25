using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 22 February 2021 16:45:11
* @Email: 326308290@qq.com
*/
    public class ShardingParallelDbContextFactoryManager:IShardingParallelDbContextFactoryManager
    {
        private readonly Dictionary<>
        public DbContext CreateDbContext(string connectKey, string tail)
        {
            throw new NotImplementedException();
        }
    }
}