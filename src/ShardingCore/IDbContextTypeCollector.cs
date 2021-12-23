using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    public interface IDbContextTypeCollector
    {
        Type ShardingDbContextType { get; }
    }

    public class DbContextTypeCollector<TShardingDbContext> : IDbContextTypeCollector
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public DbContextTypeCollector()
        {
            ShardingDbContextType = typeof(TShardingDbContext);
        }
        public Type ShardingDbContextType { get; }
    }
}
