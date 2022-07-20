using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.DbContextCreator;
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
            return Create(context, false);
        }
        public object Create(DbContext context, bool designTime)
        {
            if (context is IShardingTableDbContext shardingTableDbContext && shardingTableDbContext.RouteTail != null && !string.IsNullOrWhiteSpace(shardingTableDbContext.RouteTail.GetRouteTailIdentity()))
            {

                return $"{context.GetType()}_{shardingTableDbContext.RouteTail.GetRouteTailIdentity()}_{designTime}";
            }
            else
            {
                return (context.GetType(),designTime);
            }
        }
    }
}