using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/20 11:34:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextOptionsBuilderConfig<TShardingDbContext>  where TShardingDbContext:DbContext,IShardingDbContext
    {
        DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString, DbContextOptionsBuilder dbContextOptionsBuilder);
        DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder);
    }
}
