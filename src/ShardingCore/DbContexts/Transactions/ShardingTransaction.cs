using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Exceptions;

namespace ShardingCore.DbContexts.Transactions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 18 December 2020 10:23:42
* @Email: 326308290@qq.com
*/
    public class ShardingTransaction : IShardingTransaction
    {
        private IDbContextTransaction _dbTransaction;
        private bool _isOpened;
        public bool IsOpened => _isOpened;
        public bool IsUsed => _dbTransaction != null;
        private readonly ISet<DbContext> _dbContextUseTransactions = new HashSet<DbContext>();

        public void Use(DbContext dbContext)
        {
            if (!_isOpened)
                throw new ShardingTransactionException($"{nameof(ShardingTransaction)} is not open");
            if (!_dbContextUseTransactions.Contains(dbContext))
            {
                if (!IsUsed)
                    _dbTransaction = dbContext.Database.BeginTransaction();
                else
                    dbContext.Database.UseTransaction(_dbTransaction.GetDbTransaction());
                _dbContextUseTransactions.Add(dbContext);
            }
        }

        public void Open()
        {
            _isOpened = true;
        }

        private void Close()
        {
            _isOpened = false;
            _dbContextUseTransactions.Clear();
        }

        public void Rollback()
        {
            _dbTransaction?.Rollback();
        }

#if EFCORE2
        public Task RollbackAsync()
        {
            _dbTransaction?.Rollback();
            return Task.CompletedTask;
        }
#endif

#if !EFCORE2
        public async Task RollbackAsync()
        {
            if (_dbTransaction != null)
                await _dbTransaction.RollbackAsync();
        }
#endif

        public void Commit()
        {
            _dbTransaction?.Commit();
        }

#if EFCORE2
        public Task CommitAsync()
        {
            _dbTransaction?.Commit();
            return Task.CompletedTask;
        }
#endif
#if !EFCORE2
        public async Task CommitAsync()
        {
            if (_dbTransaction != null)
                await _dbTransaction.CommitAsync();
        }
#endif

        public IDbContextTransaction GetDbContextTransaction()
        {
            return _dbTransaction;
        }

        public void Dispose()
        {
            Close();
            _dbTransaction?.Dispose();
            _dbTransaction = null;
        }
    }
}