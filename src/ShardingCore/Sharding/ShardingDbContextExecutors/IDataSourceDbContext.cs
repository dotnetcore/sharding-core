using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.DbContexts.ShardingDbContexts;

namespace ShardingCore.Sharding.ShardingDbContextExecutors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/19 11:07:57
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IDataSourceDbContext : IDisposable
#if !EFCORE2
        , IAsyncDisposable
#endif
    {
        bool IsDefault { get; }
        DbContext CreateDbContext(IRouteTail routeTail);
        void UseTransaction(IDbContextTransaction dbContextTransaction);

        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,CancellationToken cancellationToken = new CancellationToken());



        void Rollback();
        void Commit();
#if !EFCORE2
        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
#endif

    }
}
