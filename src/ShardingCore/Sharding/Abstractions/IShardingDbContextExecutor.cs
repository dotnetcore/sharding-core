using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts;
using ShardingCore.Sharding.ShardingTransactions;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 9:50:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingDbContextExecutor : IDisposable
#if !EFCORE2
        , IAsyncDisposable

#endif
    {
        IShardingTransaction CurrentShardingTransaction { get; }
        bool IsBeginTransaction { get; }
        /// <summary>
        /// 读写分离优先级
        /// </summary>
        int ReadWriteSeparationPriority { get; set; }
        /// <summary>
        /// 当前是否开启读写分离
        /// </summary>
        bool ReadWriteSeparation { get; set; }

        /// <summary>
        /// 是否使用了读写分离
        /// </summary>
        /// <returns></returns>
        bool IsUseReadWriteSeparation();

        bool EnableAutoTrack();
        /// <summary>
        /// create sharding db context options
        /// </summary>
        /// <param name="parallelQuery">this query has >1 connection query</param>
        /// <param name="dataSourceName">data source name</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext CreateDbContext(bool parallelQuery, string dataSourceName, IRouteTail routeTail);

        DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class;

        IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);


        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken());

        int SaveChanges(bool acceptAllChangesOnSuccess);

        void ClearTransaction();
        Task ClearTransactionAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}
