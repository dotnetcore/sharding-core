#if EFCORE2
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


    public class ShardingRelationalTransactionFactory: RelationalTransactionFactory
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
            return new ShardingRelationalTransaction(connection, transaction, logger,
                transactionOwned);
        }
    }
}
#endif