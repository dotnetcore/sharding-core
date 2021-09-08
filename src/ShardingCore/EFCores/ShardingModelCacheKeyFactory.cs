using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Abstractions;

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
            if (context is IShardingTableDbContext shardingTableDbContext&&!string.IsNullOrWhiteSpace(shardingTableDbContext.RouteTail.GetRouteTailIdentity()))
            {
                
                return $"{context.GetType()}_{shardingTableDbContext.RouteTail.GetRouteTailIdentity()}";
            }
            else
            {
                return context.GetType();
            }
        }
    }
}