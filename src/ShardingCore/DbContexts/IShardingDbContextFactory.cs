using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 08:22:23
* @Email: 326308290@qq.com
*/
    public interface IShardingDbContextFactory
    {
        DbContext Create(ShardingDbContextOptions shardingDbContextOptions);
        //DbContext Create(DbConnection dbConnection,string tail);
    }
}