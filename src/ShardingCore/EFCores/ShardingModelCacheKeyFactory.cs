using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore.EFCores
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 16:13:05
* @Email: 326308290@qq.com
*/
    public class ShardingModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            if (context is IShardingTableDbContext shardingTableDbContext&&!string.IsNullOrWhiteSpace(shardingTableDbContext.ModelChangeKey))
            {
                
                return $"{context.GetType()}_{shardingTableDbContext.ModelChangeKey}";
            }
            else
            {
                return context.GetType();
            }
        }
    }
}