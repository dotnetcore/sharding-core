#if EFCORE3
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/5 20:37:36
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRelationalTransaction : RelationalTransaction
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;
        public ShardingRelationalTransaction(IShardingDbContext shardingDbContext, IRelationalConnection connection,
            DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection,
            transaction, transactionId, logger, transactionOwned)
        {
            _shardingDbContext = shardingDbContext ??
                                 throw new ShardingCoreInvalidOperationException(
                                     $"should implement {nameof(IShardingDbContext)}");
            _shardingDbContextExecutor = shardingDbContext.GetShardingExecutor() ??
                                         throw new ShardingCoreInvalidOperationException(
                                             $"{shardingDbContext.GetType()} cant get {nameof(IShardingDbContextExecutor)} from {nameof(shardingDbContext.GetShardingExecutor)}");
        }

        public override void Commit()
        {
            base.Commit();
            _shardingDbContextExecutor.Commit();
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }

        public override void Rollback()
        {
            base.Rollback();
            _shardingDbContextExecutor.Rollback();
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }

#if !EFCORE2
        public override async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.RollbackAsync(cancellationToken);

            await _shardingDbContextExecutor.RollbackAsync(cancellationToken);
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.CommitAsync(cancellationToken);

            await _shardingDbContextExecutor.CommitAsync(cancellationToken);
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }
#endif
    }
}
#endif