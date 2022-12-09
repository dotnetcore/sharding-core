#if EFCORE2
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/20 10:08:42
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// manage transaction
    /// </summary>
    public class ShardingRelationalTransactionManager : IRelationalTransactionManager
    {
        private readonly IRelationalConnection _relationalConnection;
        private readonly ICurrentDbContext _currentDbContext;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;



        public ShardingRelationalTransactionManager(IRelationalConnection relationalConnection,ICurrentDbContext currentDbContext)
        {
            _relationalConnection = relationalConnection;
            _currentDbContext = currentDbContext;
            _shardingDbContext = currentDbContext.Context as IShardingDbContext??throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
            _shardingDbContextExecutor = _shardingDbContext.GetShardingExecutor();
        }

        public void ResetState()
        {
            _relationalConnection.ResetState();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);
        }

        public void CommitTransaction()
        {
            _relationalConnection.CommitTransaction();
        }


        public void RollbackTransaction()
        {
            _relationalConnection.RollbackTransaction();
        }


        public IDbContextTransaction CurrentTransaction => _relationalConnection.CurrentTransaction;
        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var dbContextTransaction = _relationalConnection.BeginTransaction(isolationLevel);
            
            if (dbContextTransaction is ShardingRelationalTransaction shardingRelationalTransaction)
            {
                shardingRelationalTransaction.SetShardingDbContext(_shardingDbContext);
            }
            _shardingDbContextExecutor.NotifyShardingTransaction();
            return dbContextTransaction;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var dbContextTransaction = await _relationalConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
            
            if (dbContextTransaction is ShardingRelationalTransaction shardingRelationalTransaction)
            {
                shardingRelationalTransaction.SetShardingDbContext(_shardingDbContext);
            }
            _shardingDbContextExecutor.NotifyShardingTransaction();
            return dbContextTransaction;
        }

        public IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            var dbContextTransaction = _relationalConnection.UseTransaction(transaction);
            _shardingDbContextExecutor.NotifyShardingTransaction();
            return dbContextTransaction;
        }
    }
}

#endif