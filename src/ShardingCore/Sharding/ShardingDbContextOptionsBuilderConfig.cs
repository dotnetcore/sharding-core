using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/19 20:57:52
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingDbContextOptionsBuilderConfig<TShardingDbContext> : IShardingDbContextOptionsBuilderConfig where TShardingDbContext : DbContext, IShardingDbContext
    {
        public ShardingDbContextOptionsBuilderConfig(Action<DbConnection, DbContextOptionsBuilder> sameConnectionDbContextOptionsCreator, Action<DbContextOptionsBuilder> defaultQueryDbContextOptionsCreator)
        {
            SameConnectionDbContextOptionsCreator = sameConnectionDbContextOptionsCreator;
            DefaultQueryDbContextOptionsCreator = defaultQueryDbContextOptionsCreator;
        }
        public Action<DbConnection, DbContextOptionsBuilder> SameConnectionDbContextOptionsCreator { get; }
        public Action<DbContextOptionsBuilder> DefaultQueryDbContextOptionsCreator { get; }
        public Type ShardingDbContextType => typeof(TShardingDbContext);

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            SameConnectionDbContextOptionsCreator(dbConnection, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding<TShardingDbContext>();
            return dbContextOptionsBuilder;
        }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            DefaultQueryDbContextOptionsCreator(dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding<TShardingDbContext>();
            return dbContextOptionsBuilder;
        }
    }
}
