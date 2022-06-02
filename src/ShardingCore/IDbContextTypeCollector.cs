using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /// <summary>
    /// 启动dbContext类型收集器用于初始化确定dbcontext 类型
    /// </summary>
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
