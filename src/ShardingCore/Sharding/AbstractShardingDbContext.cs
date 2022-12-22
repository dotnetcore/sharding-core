using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.EFCores;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 09:57:08
    * @Email: 326308290@qq.com
    */

    /// <summary>
    /// 分表分库的dbcontext
    /// </summary>
    public abstract class AbstractShardingDbContext : DbContext, IShardingDbContext
    {
        protected IShardingDbContextExecutor ShardingDbContextExecutor { get; }


        public AbstractShardingDbContext(DbContextOptions options) : base(options)
        {
            var wrapOptionsExtension = options.FindExtension<ShardingWrapOptionsExtension>();
            if (wrapOptionsExtension != null)
            {
                ShardingDbContextExecutor = new ShardingDbContextExecutor(this);
            }
        }
        
        public IShardingDbContextExecutor GetShardingExecutor()
        {
            return ShardingDbContextExecutor;
        }

        public override void Dispose()
        {
            ShardingDbContextExecutor?.Dispose();
            base.Dispose();
        }
#if !EFCORE2

        public override async ValueTask DisposeAsync()
        {
            if (ShardingDbContextExecutor!=null)
            {
                await ShardingDbContextExecutor.DisposeAsync();
            }

            await base.DisposeAsync();
        }
#endif
    }
}