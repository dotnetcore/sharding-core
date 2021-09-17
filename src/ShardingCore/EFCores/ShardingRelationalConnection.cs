using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/5 15:41:20
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingRelationalConnection : IRelationalConnection
    {
        private readonly IRelationalConnection _relationalConnection;


        public ShardingRelationalConnection(IRelationalConnection _relationalConnection, DbTransaction transaction)
        {
            this._relationalConnection = _relationalConnection;
            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
        }

        public void ResetState()
        {
            _relationalConnection.ResetState();
        }

        public Task ResetStateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _relationalConnection.ResetStateAsync(cancellationToken);
        }


        public IDbContextTransaction BeginTransaction()
        {
            return _relationalConnection.BeginTransaction();
        }

        public  Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _relationalConnection.BeginTransactionAsync(cancellationToken);
        }

        public void CommitTransaction()
        {
            _relationalConnection.CommitTransaction();
        }


        public void RollbackTransaction()
        {
            _relationalConnection.RollbackTransaction();
        }
#if EFCORE5
        public IDbContextTransaction UseTransaction(DbTransaction transaction, Guid transactionId)
        {
        var dbContextTransaction = _relationalConnection.UseTransaction(transaction, transactionId);
            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
            return dbContextTransaction;
        }
        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, Guid transactionId,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, transactionId, cancellationToken);
            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
            return dbContextTransaction;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _relationalConnection.CommitTransactionAsync(cancellationToken);
        }
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _relationalConnection.RollbackTransactionAsync(cancellationToken);
        }
#endif

#if !EFCORE5
        public bool IsMultipleActiveResultSetsEnabled => _relationalConnection.IsMultipleActiveResultSetsEnabled;

        
# endif
        IDbContextTransaction IRelationalConnection.CurrentTransaction => _relationalConnection.CurrentTransaction;

        IDbContextTransaction IDbContextTransactionManager.CurrentTransaction => _relationalConnection.CurrentTransaction;


        public SemaphoreSlim Semaphore => _relationalConnection.Semaphore;

        public bool Open(bool errorsExpected = false)
        {
            return _relationalConnection.Open(errorsExpected);
        }

        public Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            return _relationalConnection.OpenAsync(cancellationToken, errorsExpected);
        }

        public bool Close()
        {
            return _relationalConnection.Close();
        }



        public DbConnection DbConnection => _relationalConnection.DbConnection;

        public DbContext Context =>
            _relationalConnection.Context;
        public Guid ConnectionId => _relationalConnection.ConnectionId;

        public int? CommandTimeout
        {
            get
            {
                return _relationalConnection.CommandTimeout;
            }
            set
            {
                _relationalConnection.CommandTimeout = value;
            }
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _relationalConnection.BeginTransaction(isolationLevel);
        }

        public  Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return  _relationalConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            var dbContextTransaction = _relationalConnection.UseTransaction(transaction);
            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
            return dbContextTransaction;
        }


        public void Dispose()
        {
            _relationalConnection.Dispose();
        }


        public string ConnectionString => _relationalConnection.ConnectionString;

        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = new CancellationToken())
        {
            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, cancellationToken);
            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
            return dbContextTransaction;

        }

        public Task<bool> CloseAsync()
        {
            return _relationalConnection.CloseAsync();
        }

        public ValueTask DisposeAsync()
        {
            return _relationalConnection.DisposeAsync();
        }
    }
}
