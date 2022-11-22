using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Core.DbContextCreator;

namespace ShardingCore.Sharding.ShardingDbContextExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/19 11:07:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 同数据源下的dbcontext管理者
    /// </summary>
    public interface IDataSourceDbContext : IDisposable
#if !EFCORE2
        , IAsyncDisposable
#endif
    {
        /// <summary>
        /// is default data source connection string
        /// </summary>
        bool IsDefault { get; }
        int DbContextCount { get; }
        DbContext CreateDbContext(IRouteTail routeTail);

        /// <summary>
        /// 通知事务自动管理是否要清理还是开启还是加入事务
        /// </summary>
        void NotifyTransaction();

        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,CancellationToken cancellationToken = new CancellationToken());

        IDictionary<string, DbContext> GetCurrentContexts();

        void Rollback();
        void Commit();
#if !EFCORE2
        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
#if !EFCORE3&&!NETSTANDARD2_0
        void CreateSavepoint(string name);
        Task CreateSavepointAsync(string name, CancellationToken cancellationToken = new CancellationToken());
        void RollbackToSavepoint(string name);
        Task RollbackToSavepointAsync(string name,CancellationToken cancellationToken = default(CancellationToken));
        void ReleaseSavepoint(string name);
        Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default(CancellationToken));
#endif
#endif

    }
}
