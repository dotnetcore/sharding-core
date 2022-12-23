using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 9:50:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextExecutor : IShardingTransaction,IReadWriteSwitch,ICurrentDbContextDiscover,IDisposable
#if !EFCORE2
        , IAsyncDisposable

#endif
    {
        /// <summary>
        /// 使用对象创建db context的前执行
        /// </summary>
        event EventHandler<EntityCreateDbContextBeforeEventArgs> EntityCreateDbContextBefore;
        /// <summary>
        /// 使用对象创建db context的后执行
        /// </summary>
        event EventHandler<EntityCreateDbContextAfterEventArgs> EntityCreateDbContextAfter;
        /// <summary>
        /// 使用tail创建db context的前执行
        /// </summary>
        event EventHandler<CreateDbContextBeforeEventArgs> CreateDbContextBefore;
        /// <summary>
        /// 使用tail创建db context的后执行
        /// </summary>
        event EventHandler<CreateDbContextAfterEventArgs> CreateDbContextAfter;
        /// <summary>
        /// has multi db context
        /// </summary>
        bool IsMultiDbContext { get; }


        /// <summary>
        /// create sharding db context options
        /// </summary>
        /// <param name="strategy">如果当前查询需要多链接的情况下那么将使用<code>IndependentConnectionQuery</code>否则使用<code>ShareConnection</code></param>
        /// <param name="dataSourceName">data source name</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext CreateDbContext(CreateDbContextStrategyEnum strategy, string dataSourceName, IRouteTail routeTail);
        /// <summary>
        /// create db context by entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class;

        IVirtualDataSource GetVirtualDataSource();

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken());

        int SaveChanges(bool acceptAllChangesOnSuccess);

        DbContext GetShellDbContext();

    }
}
