using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    public class ShardingDbContextOptionsBuilderConfigure<TShardingDbContext>  where TShardingDbContext : DbContext, IShardingDbContext
    {
        public ShardingDbContextOptionsBuilderConfigure(Action<DbConnection, DbContextOptionsBuilder> sameConnectionDbContextOptionsCreator, Action<string, DbContextOptionsBuilder> defaultQueryDbContextOptionsCreator)
        {
            SameConnectionDbContextOptionsCreator = sameConnectionDbContextOptionsCreator;
            DefaultQueryDbContextOptionsCreator = defaultQueryDbContextOptionsCreator;
        }
        public Action<DbConnection, DbContextOptionsBuilder> SameConnectionDbContextOptionsCreator { get; }
        public Action<string, DbContextOptionsBuilder> DefaultQueryDbContextOptionsCreator { get; }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(DbConnection dbConnection, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            SameConnectionDbContextOptionsCreator(dbConnection, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding();
            return dbContextOptionsBuilder;
        }

        public DbContextOptionsBuilder UseDbContextOptionsBuilder(string connectionString, DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            DefaultQueryDbContextOptionsCreator(connectionString, dbContextOptionsBuilder);
            dbContextOptionsBuilder.UseInnerDbContextSharding();
            return dbContextOptionsBuilder;
        }
    }
}
