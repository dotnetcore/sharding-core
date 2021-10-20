using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/5 16:03:04
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
#if !EFCORE2

    public class ShardingRelationalTransactionFactory<TShardingDbContext> : RelationalTransactionFactory where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly RelationalTransactionFactoryDependencies _dependencies;
        public ShardingRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies) : base(dependencies)
        {
            _dependencies = dependencies;
        }
        public override RelationalTransaction Create(IRelationalConnection connection, DbTransaction transaction, Guid transactionId,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger, bool transactionOwned)
        {
            var supportShardingTransaction = connection.Context as ISupportShardingTransaction;
            return new ShardingRelationalTransaction(supportShardingTransaction, connection, transaction, transactionId, logger, transactionOwned);
        }
    }
#endif
#if EFCORE2

    public class ShardingRelationalTransactionFactory<TShardingDbContext> : RelationalTransactionFactory where TShardingDbContext : DbContext, IShardingDbContext
    {
        private readonly RelationalTransactionFactoryDependencies _dependencies;
        public ShardingRelationalTransactionFactory(RelationalTransactionFactoryDependencies dependencies) : base(dependencies)
        {
            _dependencies = dependencies;
        }
        public override RelationalTransaction Create(IRelationalConnection connection, DbTransaction transaction
            , IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
        {
            var supportShardingTransaction = GetDbContext(connection) as ISupportShardingTransaction;
            return new ShardingRelationalTransaction(supportShardingTransaction, connection, transaction, logger,
                transactionOwned);
        }

        private DbContext GetDbContext(IRelationalConnection connection)
        {
            var namedConnectionStringResolver = ((RelationalConnectionDependencies)connection.GetPropertyValue("Dependencies")).ConnectionStringResolver;
            var serviceProvider = (IServiceProvider)namedConnectionStringResolver.GetPropertyValue("ApplicationServiceProvider");
            var dbContext = (DbContext)serviceProvider.GetService(typeof(TShardingDbContext));
            return dbContext;
        }
    }
#endif
}
