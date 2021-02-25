using System;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.DbContexts.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 07 February 2021 15:24:41
* @Email: 326308290@qq.com
*/
    public class AbstractionShardingDbContext:DbContext
    {
        public AbstractionShardingDbContext(ShardingDbContextOptionBuilder<AbstractionShardingDbContext> optionBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}