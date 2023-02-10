using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.EFCores;
using ShardingCore.Extensions;

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
        private bool _createExecutor = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        protected AbstractShardingDbContext(DbContextOptions options) : base(options)
        {
           
        }

        private IShardingDbContextExecutor _shardingDbContextExecutor;
        public IShardingDbContextExecutor GetShardingExecutor()
        {
            if (!_createExecutor)
            {
                _shardingDbContextExecutor=this.CreateShardingDbContextExecutor();
                _createExecutor = true;
            }
            return _shardingDbContextExecutor;
        }

        /// <summary>
        /// 当前dbcontext是否是执行的dbcontext
        /// </summary>
        public bool IsExecutor => GetShardingExecutor() == default;

        public override void Dispose()
        {
            _shardingDbContextExecutor?.Dispose();
            base.Dispose();
        }
#if !EFCORE2

        public override async ValueTask DisposeAsync()
        {
            if (_shardingDbContextExecutor!=null)
            {
                await _shardingDbContextExecutor.DisposeAsync();
            }

            await base.DisposeAsync();
        }
#endif
    }
}