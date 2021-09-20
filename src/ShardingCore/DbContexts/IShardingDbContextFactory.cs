using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 24 December 2020 08:22:23
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextFactory<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        DbContext Create(ShardingDbContextOptions shardingDbContextOptions);
    }
}