using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Exceptions;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 13 August 2021 20:50:01
* @Email: 326308290@qq.com
*/
    public static class ShardingDoExtension
    {
        public static Task ShardingAddAsync<T>(this DbContext dbContext,T entity) where T : class
        {
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                dbContext.AddAsync()
            }
            else
            {
                
                throw new ShardingCoreException($"dbContext:[{dbContext.GetType().FullName}] is not from IShardingTableDbContext" )
            }
        }
    }
}