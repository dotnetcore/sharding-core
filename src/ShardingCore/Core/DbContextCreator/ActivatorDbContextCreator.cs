using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    /// <summary>
    /// dbcontext的创建者
    /// 反射创建默认框架采用这个可以，
    /// 如果需要dbcontext构造函数支持依赖注入参数
    /// 可以自行重写这个接口
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/2 21:15:09
    /// Email: 326308290@qq.com
    public class ActivatorDbContextCreator<TShardingDbContext>: IDbContextCreator where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly Func<ShardingDbContextOptions, DbContext> _creator;
        public ActivatorDbContextCreator()
        {
            ShardingCoreHelper.CheckContextConstructors<TShardingDbContext>();
            _creator = ShardingCoreHelper.CreateActivator<TShardingDbContext>();
        }
        /// <summary>
        /// 如何创建dbcontext
        /// </summary>
        /// <param name="shellDbContext"></param>
        /// <param name="shardingDbContextOptions"></param>
        /// <returns></returns>
        public virtual DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var dbContext = _creator(shardingDbContextOptions);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
            }
            _ = dbContext.Model;
            return dbContext;
        }

        public virtual DbContext GetShellDbContext(IShardingProvider shardingProvider)
        {
            return shardingProvider.GetService<TShardingDbContext>();
        }
    }
}
