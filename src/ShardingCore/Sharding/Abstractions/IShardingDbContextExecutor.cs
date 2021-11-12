using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        /// <summary>
        /// 读写分离优先级
        /// </summary>
        int ReadWriteSeparationPriority { get; set; }
        /// <summary>
        /// 当前是否开启读写分离
        /// </summary>
        bool ReadWriteSeparation { get; set; }
        /// <summary>
        /// 是否存在多个db context
        /// </summary>
        bool IsMultiDbContext { get; }


        /// <summary>
        /// create sharding db context options
        /// </summary>
        /// <param name="parallelQuery">this query has >1 connection query</param>
        /// <param name="dataSourceName">data source name</param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        DbContext CreateDbContext(bool parallelQuery, string dataSourceName, IRouteTail routeTail);

        DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class;



        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken());

        int SaveChanges(bool acceptAllChangesOnSuccess);
        void NotifyShardingTransaction();




        void Rollback();
        void Commit();
#if !EFCORE2
        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
#endif
    }
}
