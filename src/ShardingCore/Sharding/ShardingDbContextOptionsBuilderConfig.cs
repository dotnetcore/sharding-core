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
        public ShardingDbContextOptionsBuilderConfig(Action<DbConnection, DbContextOptionsBuilder> shardingDbContextConnectionOptionsCreator,Action<string, DbContextOptionsBuilder> shardingDbContextStringionOptionsCreator)
        {
            ShardingDbContextConnectionOptionsCreator = shardingDbContextConnectionOptionsCreator;
            ShardingDbContextStringOptionsCreator = shardingDbContextStringionOptionsCreator;
        }
        public Action<DbConnection, DbContextOptionsBuilder> ShardingDbContextConnectionOptionsCreator { get; }
        public Action<string, DbContextOptionsBuilder> ShardingDbContextStringOptionsCreator { get; }
        public Type ShardingDbContextType => typeof(TShardingDbContext);
        public bool SupportMARS => ShardingDbContextStringOptionsCreator == null;

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            ShardingDbContextConnectionOptionsCreator(dbConnection, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding<TShardingDbContext>();
            return dbContextOptionsBuilder;
        }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            ShardingDbContextStringOptionsCreator(connectionString, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding<TShardingDbContext>();
            return dbContextOptionsBuilder;
        }
    }
}
