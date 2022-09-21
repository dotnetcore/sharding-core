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
#if !EFCORE2 && !EFCORE3 && !EFCORE5 && !EFCORE6
    error
#endif
    public class ShardingRelationalTransaction : RelationalTransaction
    {
        private readonly IShardingDbContext _shardingDbContext;
#if EFCORE6
        public ShardingRelationalTransaction(IShardingDbContext shardingDbContext, IRelationalConnection connection, DbTransaction transaction, Guid transactionId, IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned, ISqlGenerationHelper sqlGenerationHelper) : base(connection, transaction, transactionId, logger, transactionOwned, sqlGenerationHelper)
        {
            _shardingDbContext = shardingDbContext ?? throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
        }

#endif
#if EFCORE3 || EFCORE5
        public ShardingRelationalTransaction(IShardingDbContext shardingDbContext, IRelationalConnection connection, DbTransaction transaction, Guid transactionId, IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, transactionId, logger, transactionOwned)
        {
            _shardingDbContext = shardingDbContext??throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
        }

#endif
#if EFCORE2
        public ShardingRelationalTransaction(IShardingDbContext shardingDbContext, IRelationalConnection connection, DbTransaction transaction,IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, logger, transactionOwned)
        {
            _shardingDbContext = shardingDbContext??throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
        }

#endif
        //protected override void ClearTransaction()
        //{
        //    if (_canClear)
        //    {
        //        base.ClearTransaction();
        //        _supportShardingTransaction.NotifyShardingTransaction(null);
        //    }
        //}


        public override void Commit()
        {
            base.Commit();
            _shardingDbContext.Commit();
            _shardingDbContext.NotifyShardingTransaction();
        }

        public override void Rollback()
        {
            base.Rollback();
            _shardingDbContext.Rollback();
            _shardingDbContext.NotifyShardingTransaction();
        }

#if !EFCORE2
        
        public override async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.RollbackAsync(cancellationToken);

            await _shardingDbContext.RollbackAsync(cancellationToken);
            _shardingDbContext.NotifyShardingTransaction();
        }

        public override async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await base.CommitAsync(cancellationToken);

            await _shardingDbContext.CommitAsync(cancellationToken);
            _shardingDbContext.NotifyShardingTransaction();
        }
#endif
    }
}
