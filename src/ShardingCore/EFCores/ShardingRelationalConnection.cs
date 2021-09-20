//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Internal;
//using Microsoft.EntityFrameworkCore.Query.Internal;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.EntityFrameworkCore.Storage.Internal;
//using Microsoft.Extensions.DependencyInjection;
//using ShardingCore.Extensions;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ShardingTransactions;

//namespace ShardingCore.EFCores
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/5 15:41:20
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public class ShardingRelationalConnection : IRelationalConnection
//    {
//        private readonly IRelationalConnection _relationalConnection;

//        private readonly ISupportShardingTransaction _supportShardingTransaction;
//        public ShardingRelationalConnection(IRelationalConnection _relationalConnection, DbTransaction transaction)
//        {
//            this._relationalConnection = _relationalConnection;
//            if (Context is ISupportShardingTransaction supportShardingTransaction)
//            {
//                _supportShardingTransaction = supportShardingTransaction;
//            }
//        }

//        public void ResetState()
//        {
//            _relationalConnection.ResetState();
//        }

//        public Task ResetStateAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return _relationalConnection.ResetStateAsync(cancellationToken);
//        }


//        public IDbContextTransaction BeginTransaction()
//        {
//            var dbContextTransaction = _relationalConnection.BeginTransaction();
//            _supportShardingTransaction?.BeginTransaction();
//            return dbContextTransaction;
//        }

//        public  async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.BeginTransactionAsync(cancellationToken);
//            _supportShardingTransaction?.BeginTransaction();
//            return dbContextTransaction;
//        }

//        public void CommitTransaction()
//        {
//            _relationalConnection.CommitTransaction();
//            _supportShardingTransaction?.Commit();
//        }


//        public void RollbackTransaction()
//        {
//            _relationalConnection.RollbackTransaction();
//            _supportShardingTransaction?.Rollback();
//        }
//#if EFCORE5
//        public IDbContextTransaction UseTransaction(DbTransaction transaction, Guid transactionId)
//        {
//        var dbContextTransaction = _relationalConnection.UseTransaction(transaction, transactionId);
//        _supportShardingTransaction?.UseTransaction(transaction);
//            return dbContextTransaction;
//        }
//        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, Guid transactionId,
//            CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, transactionId, cancellationToken);
//            _supportShardingTransaction?.UseTransaction(transaction);
//            return dbContextTransaction;
//        }

//        public async Task CommitTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            await _relationalConnection.CommitTransactionAsync(cancellationToken);

//            if (_supportShardingTransaction != null)
//                await _supportShardingTransaction.CommitAsync(cancellationToken);

//        }
//        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            await _relationalConnection.RollbackTransactionAsync(cancellationToken);
//            if (_supportShardingTransaction != null)
//                await _supportShardingTransaction.RollbackAsync(cancellationToken);
//        }
//#endif

//#if !EFCORE5
//        public bool IsMultipleActiveResultSetsEnabled => _relationalConnection.IsMultipleActiveResultSetsEnabled;

        
//# endif
//        IDbContextTransaction IRelationalConnection.CurrentTransaction => _relationalConnection.CurrentTransaction;

//        IDbContextTransaction IDbContextTransactionManager.CurrentTransaction => _relationalConnection.CurrentTransaction;


//        public SemaphoreSlim Semaphore => _relationalConnection.Semaphore;

//        public bool Open(bool errorsExpected = false)
//        {
//            return _relationalConnection.Open(errorsExpected);
//        }

//        public Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
//        {
//            return _relationalConnection.OpenAsync(cancellationToken, errorsExpected);
//        }

//        public bool Close()
//        {
//            return _relationalConnection.Close();
//        }



//        public DbConnection DbConnection => _relationalConnection.DbConnection;

//        public DbContext Context =>
//            _relationalConnection.Context;
//        public Guid ConnectionId => _relationalConnection.ConnectionId;

//        public int? CommandTimeout
//        {
//            get
//            {
//                return _relationalConnection.CommandTimeout;
//            }
//            set
//            {
//                _relationalConnection.CommandTimeout = value;
//            }
//        }

//        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
//        {
//            var dbContextTransaction = _relationalConnection.BeginTransaction(isolationLevel);
//            _supportShardingTransaction?.BeginTransaction(isolationLevel);
//            return dbContextTransaction;
//        }

//        public  async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
//            CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await  _relationalConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
//            _supportShardingTransaction?.BeginTransaction(isolationLevel);
//            return dbContextTransaction;
//        }

//        public IDbContextTransaction UseTransaction(DbTransaction transaction)
//        {
//            var dbContextTransaction = _relationalConnection.UseTransaction(transaction);
//            _supportShardingTransaction?.UseTransaction(transaction);
//            return dbContextTransaction;
//        }


//        public void Dispose()
//        {
//            _relationalConnection.Dispose();
//        }


//        public string ConnectionString => _relationalConnection.ConnectionString;

//        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, cancellationToken);
//            ((IShardingTransaction)Context).UseShardingTransaction(transaction);
//            return dbContextTransaction;

//        }

//        public Task<bool> CloseAsync()
//        {
//            return _relationalConnection.CloseAsync();
//        }

//        public ValueTask DisposeAsync()
//        {
//            return _relationalConnection.DisposeAsync();
//        }
//    }
//}
