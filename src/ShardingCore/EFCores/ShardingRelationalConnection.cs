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


//#if !EFCORE2
//        public ShardingRelationalConnection(IRelationalConnection _relationalConnection, DbTransaction transaction)
//        {
//            this._relationalConnection = _relationalConnection;
//            if (Context is ISupportShardingTransaction supportShardingTransaction)
//            {
//                _supportShardingTransaction = supportShardingTransaction;
//            }
//        }

//#endif
//#if EFCORE2
//        private readonly Type _dbContextType;
//        public ShardingRelationalConnection(IRelationalConnection _relationalConnection,DbTransaction transaction,Type dbContextType)
//        {
//            this._relationalConnection = _relationalConnection;
//            _dbContextType = dbContextType;
//            if (Context is ISupportShardingTransaction supportShardingTransaction)
//            {
//                _supportShardingTransaction = supportShardingTransaction;
//            }
//        }
//#endif

//        public void ResetState()
//        {
//            _relationalConnection.ResetState();
//        }

//#if !EFCORE2
//        public Task ResetStateAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return _relationalConnection.ResetStateAsync(cancellationToken);
//        }

//#endif

//        public IDbContextTransaction BeginTransaction()
//        {
//            var dbContextTransaction = _relationalConnection.BeginTransaction();
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }

//        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.BeginTransactionAsync(cancellationToken);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }

//        public void CommitTransaction()
//        {
//            _relationalConnection.CommitTransaction();
//        }


//        public void RollbackTransaction()
//        {
//            _relationalConnection.RollbackTransaction();
//        }
//#if EFCORE5
//        public IDbContextTransaction NotifyTransaction(DbTransaction transaction, Guid transactionId)
//        {
//            var dbContextTransaction = _relationalConnection.NotifyTransaction(transaction, transactionId);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }
//        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, Guid transactionId,
//            CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, transactionId, cancellationToken);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }

//        public Task CommitTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return _relationalConnection.CommitTransactionAsync(cancellationToken);
//        }
//        public Task RollbackTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return _relationalConnection.RollbackTransactionAsync(cancellationToken);
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
//#if !EFCORE2
//            _relationalConnection.Context;
//#endif
//#if EFCORE2
//            GetDbContext();

//        private DbContext GetDbContext()
//        {
//            var namedConnectionStringResolver = ((RelationalConnectionDependencies)_relationalConnection.GetPropertyValue("Dependencies")).ConnectionStringResolver;
//            var serviceProvider = (IServiceProvider)namedConnectionStringResolver.GetPropertyValue("ApplicationServiceProvider");
//            var dbContext = (DbContext)serviceProvider.GetService(_dbContextType);
//            return dbContext;
//        }


//        public void RegisterBufferable(IBufferable bufferable)
//        {
//            _relationalConnection.RegisterBufferable(bufferable);
//        }

//        public Task RegisterBufferableAsync(IBufferable bufferable, CancellationToken cancellationToken)
//        {
//            return _relationalConnection.RegisterBufferableAsync(bufferable, cancellationToken);
//        }
//#endif
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
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }

//        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
//            CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction=await  _relationalConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }

//        public IDbContextTransaction NotifyTransaction(DbTransaction transaction)
//        {
//            var dbContextTransaction = _relationalConnection.NotifyTransaction(transaction);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
//            return dbContextTransaction;
//        }


//        public void Dispose()
//        {
//            _relationalConnection.Dispose();
//        }


//        public string ConnectionString => _relationalConnection.ConnectionString;
//#if !EFCORE2

//        public async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = new CancellationToken())
//        {
//            var dbContextTransaction = await _relationalConnection.UseTransactionAsync(transaction, cancellationToken);
//            _supportShardingTransaction?.NotifyShardingTransaction(dbContextTransaction);
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
//#endif
//    }
//}
