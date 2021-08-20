using Microsoft.EntityFrameworkCore;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/3/5 17:30:10
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultShardingDbContextCreatorConfig<TShardingDbContext,TActualDbContext> : IShardingDbContextCreatorConfig
        where TShardingDbContext : DbContext, IShardingDbContext
        where TActualDbContext : DbContext, IShardingTableDbContext
    {
        private readonly Func<ShardingDbContextOptions, DbContext> _creator;
        public DefaultShardingDbContextCreatorConfig(Type actualDbContextType)
        {
            ActualDbContextType = actualDbContextType;
            _creator = ShardingCoreHelper.CreateActivator<TActualDbContext>();
        }

        public Type ShardingDbContextType => typeof(TShardingDbContext);
        public Type ActualDbContextType { get; }
        public DbContext Creator(ShardingDbContextOptions shardingDbContextOptions)
        {
            return _creator(shardingDbContextOptions);
        }
    }
}
