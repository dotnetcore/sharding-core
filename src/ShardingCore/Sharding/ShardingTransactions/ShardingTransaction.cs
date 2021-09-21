using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.ShardingTransactions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 13:02:32
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingTransaction : IShardingTransaction
    {
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;

        private readonly ConcurrentDictionary<string, IDbContextTransaction> _dbContextTransactions =
            new ConcurrentDictionary<string, IDbContextTransaction>();

        private IsolationLevel isolationLevel = IsolationLevel.Unspecified;

        private bool _isBeginTransaction = false;

        public ShardingTransaction(IShardingDbContextExecutor shardingDbContextExecutor)
        {
            _shardingDbContextExecutor = shardingDbContextExecutor;
        }

        public bool IsBeginTransaction()
        {
            return _isBeginTransaction;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_isBeginTransaction)
                throw new InvalidOperationException("transaction is already begin");
            _isBeginTransaction = true;
            this.isolationLevel = isolationLevel;

        }



        public void Use(string dataSourceName, DbContext dbContext)
        {
            if (!_isBeginTransaction)
                throw new InvalidOperationException("transaction is not begin");
            if (!_dbContextTransactions.TryGetValue(dataSourceName, out var dbContextTransaction))
            {
                dbContextTransaction = dbContext.Database.BeginTransaction(isolationLevel);
                var tryAdd = _dbContextTransactions.TryAdd(dataSourceName, dbContextTransaction);
                if (!tryAdd)
                    throw new InvalidOperationException("append transaction error");
            }
            else
            {
                dbContext.Database.UseTransaction(dbContextTransaction.GetDbTransaction());
            }
        }

        public void Rollback()
        {
            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    dbContextTransaction.Value.Rollback();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"rollback error:[{e}]");
                }
            }
            this._shardingDbContextExecutor.ClearTransaction();
        }

        public void Commit()
        {
            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    dbContextTransaction.Value.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"commit error:[{e}]");
                }
            }
            this._shardingDbContextExecutor.ClearTransaction();
        }

#if !EFCORE2

        public async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    await dbContextTransaction.Value.RollbackAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"rollback error:[{e}]");
                }
            }
            await this._shardingDbContextExecutor.ClearTransactionAsync(cancellationToken);
        }
        public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    await dbContextTransaction.Value.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"commit error:[{e}]");
                }
            }
            await this._shardingDbContextExecutor.ClearTransactionAsync(cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    await dbContextTransaction.Value.DisposeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"dispose error:[{e}]");
                }
            }
            _dbContextTransactions.Clear();
        }
#endif

        public void Dispose()
        {

            foreach (var dbContextTransaction in _dbContextTransactions)
            {
                try
                {
                    dbContextTransaction.Value.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"dispose error:[{e}]");
                }
            }
            _dbContextTransactions.Clear();
        }

    }
}
