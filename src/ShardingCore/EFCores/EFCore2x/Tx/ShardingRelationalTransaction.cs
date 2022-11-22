#if EFCORE2&&SHARDINGCORE2_6
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
        public ShardingRelationalTransaction(IShardingDbContext shardingDbContext, IRelationalConnection connection, DbTransaction transaction,IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, logger, transactionOwned)
        {
            _shardingDbContext =
 shardingDbContext??throw new ShardingCoreInvalidOperationException($"should implement {nameof(IShardingDbContext)}");
            _shardingDbContextExecutor = shardingDbContext.GetShardingExecutor() ??
                                         throw new ShardingCoreInvalidOperationException(
                                             $"{shardingDbContext.GetType()} cant get {nameof(IShardingDbContextExecutor)} from {nameof(shardingDbContext.GetShardingExecutor)}");

        }

        //protected override void ClearTransaction()
        //{
        //    if (_canClear)
        //    {
        //        base.ClearTransaction();
        //        _supportShardingTransaction.NotifyShardingTransaction(null);
        //    }
        //}f
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

    }
}
#endif