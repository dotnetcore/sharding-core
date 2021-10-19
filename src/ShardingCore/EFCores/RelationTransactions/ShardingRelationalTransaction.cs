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
        private readonly ISupportShardingTransaction _supportShardingTransaction;
        private bool supportShardingTransaction => _supportShardingTransaction != null;
#if !EFCORE2
        public ShardingRelationalTransaction(DbContext dbContext, IRelationalConnection connection, DbTransaction transaction, Guid transactionId, IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, transactionId, logger, transactionOwned)
        {
            if (dbContext is ISupportShardingTransaction supportShardingTransaction)
            {
                _supportShardingTransaction = supportShardingTransaction;
            }
        }

#endif
#if EFCORE2
        public ShardingRelationalTransaction(DbContext dbContext, IRelationalConnection connection, DbTransaction transaction,IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, logger, transactionOwned)
        {
            if (dbContext is ISupportShardingTransaction supportShardingTransaction)
            {
                _supportShardingTransaction = supportShardingTransaction;
            }
        }

#endif

        protected override void ClearTransaction()
        {
            base.ClearTransaction();
            _supportShardingTransaction.UseShardingTransaction(null);
        }


        public override void Commit()
        {
            base.Commit();
            _supportShardingTransaction?.Commit();
        }

        public override void Rollback()
        {
            base.Rollback();
            _supportShardingTransaction?.Rollback();
        }

#if !EFCORE2
        
        public override async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.RollbackAsync(cancellationToken);
            if (supportShardingTransaction)
            {
                await _supportShardingTransaction.RollbackAsync(cancellationToken);
            }
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.CommitAsync(cancellationToken);
            if (supportShardingTransaction)
            {
                await _supportShardingTransaction.CommitAsync(cancellationToken);
            }
        }
        protected override async Task ClearTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.ClearTransactionAsync(cancellationToken);
            _supportShardingTransaction.UseShardingTransaction(null);

        }
#endif
    }
}
