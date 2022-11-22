#if EFCORE2 && SHARDINGCORE2_6
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    public class ShardingRelationalTransactionManager<TShardingDbContext> : IRelationalTransactionManager where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly IRelationalConnection _relationalConnection;
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;



        public ShardingRelationalTransactionManager(IRelationalConnection relationalConnection)
        {
            _relationalConnection = relationalConnection;
            _shardingDbContext = GetDbContext(relationalConnection) as IShardingDbContext??throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
            _shardingDbContextExecutor = _shardingDbContext.GetShardingExecutor();
        }
        private DbContext GetDbContext(IRelationalConnection connection)
        {
            var namedConnectionStringResolver = ((RelationalConnectionDependencies)connection.GetPropertyValue("Dependencies")).ConnectionStringResolver;
            var serviceProvider = (IServiceProvider)namedConnectionStringResolver.GetPropertyValue("ApplicationServiceProvider");
            var dbContext = (DbContext)serviceProvider.GetService(typeof(TShardingDbContext));
            return dbContext;
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
            _shardingDbContextExecutor.NotifyShardingTransaction();
            return dbContextTransaction;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var dbContextTransaction = await _relationalConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
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