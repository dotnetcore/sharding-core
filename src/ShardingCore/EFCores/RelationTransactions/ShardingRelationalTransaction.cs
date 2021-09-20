//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Storage;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ShardingTransactions;

//namespace ShardingCore.EFCores
//{
//    /*
//    * @Author: xjm
//    * @Description:
//    * @Date: 2021/9/5 20:37:36
//    * @Ver: 1.0
//    * @Email: 326308290@qq.com
//    */
//    public class ShardingRelationalTransaction : RelationalTransaction
//    {
//        private readonly IShardingTransaction _shardingDbContext;
//        public ShardingRelationalTransaction(IRelationalConnection connection, DbTransaction transaction, Guid transactionId, IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned) : base(connection, transaction, transactionId, logger, transactionOwned)
//        {
//            _shardingDbContext = (IShardingDbContext)null;
//            _shardingDbContext.UseShardingTransaction(transaction);
//        }

//        protected override void ClearTransaction()
//        {
//            base.ClearTransaction();
//            _shardingDbContext.UseShardingTransaction(null);
//        }

//        protected override async Task ClearTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            await base.ClearTransactionAsync(cancellationToken);
//            _shardingDbContext.UseShardingTransaction(null);

//        }

//        public override void Commit()
//        {
//            base.Commit();
//        }

//        public override Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return base.CommitAsync(cancellationToken);
//        }

//        public override void Rollback()
//        {
//            base.Rollback();
//        }

//        public override Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return base.RollbackAsync(cancellationToken);
//        }
//    }
//}
