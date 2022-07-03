using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Sample.SqlServer3x.Domain.Maps;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.SqlServer3x
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/31 15:28:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IScopedService
    {

    }

    public class ScopedService : IScopedService
    {

    }

    public class CustomerDbContextCreator : IDbContextCreator
    {
        public DbContext CreateDbContext(DbContext mainDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var dbContext = new DefaultDbContext((DbContextOptions<DefaultDbContext>)shardingDbContextOptions.DbContextOptions,((DefaultDbContext)mainDbContext).ServiceProvider);
            Console.WriteLine("IsFrozen" + shardingDbContextOptions.DbContextOptions.IsFrozen);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
            }
            _ = dbContext.Model;
            return dbContext;
        }

        public DbContext GetShellDbContext(IShardingProvider shardingProvider)
        {
           return shardingProvider.GetService<DefaultDbContext>();
        }
    }

    public class DefaultDbContext : AbstractShardingDbContext, IShardingTableDbContext
    {
        public IServiceProvider ServiceProvider { get; }
        private readonly IScopedService _scopedService;

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options,IServiceProvider serviceProvider) : base(options)
        {
            ServiceProvider = serviceProvider;
            _scopedService = serviceProvider.GetRequiredService<IScopedService>();
            //Database.SetCommandTimeout(10000);
            Console.WriteLine("DefaultDbContext ctor");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysUserModAbcMap());
        }

        public IRouteTail RouteTail { get; set; }
    }
}
