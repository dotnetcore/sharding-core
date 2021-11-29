using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.DbContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 24 December 2020 08:22:48
    * @Email: 326308290@qq.com
    */
    public class ShardingDbContextFactory<TShardingDbContext> : IShardingDbContextFactory<TShardingDbContext> where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IShardingDbContextCreatorConfig<TShardingDbContext> _shardingDbContextCreatorConfig;

        public ShardingDbContextFactory(IShardingDbContextCreatorConfig<TShardingDbContext> shardingDbContextCreatorConfig)
        {
            _shardingDbContextCreatorConfig = shardingDbContextCreatorConfig;
        }
        public DbContext Create(ShardingDbContextOptions shardingDbContextOptions)
        {
            var routeTail=shardingDbContextOptions.RouteTail;
            
            var dbContext = _shardingDbContextCreatorConfig.Creator(shardingDbContextOptions);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = routeTail;
            }
            var dbContextModel = dbContext.Model;
            return dbContext;
        }

        public DbContext Create(DbContextOptions dbContextOptions, IRouteTail routeTail)
        {
            return this.Create(new ShardingDbContextOptions(dbContextOptions, routeTail));
        }
    }
}