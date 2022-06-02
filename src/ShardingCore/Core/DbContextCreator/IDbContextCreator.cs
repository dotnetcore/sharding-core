using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    /// <summary>
    /// dbcontext创建者
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/2 21:12:17
    /// Email: 326308290@qq.com
    public interface IDbContextCreator
    {
        /// <summary>
        /// 创建dbcontext
        /// </summary>
        /// <param name="shellDbContext">最外部的dbcontext也就是壳不具备真正的执行</param>
        /// <param name="shardingDbContextOptions">返回dbcontext的配置路由等信息</param>
        /// <returns></returns>
        public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions);
    }

    public interface IDbContextCreator<TShardingDbContext> : IDbContextCreator
        where TShardingDbContext : DbContext, IShardingDbContext
    {

    }
}
