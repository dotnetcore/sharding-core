using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Sharding.ShardingTransactions;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 13:30:08
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface ISupportShardingTransaction
    {
        IShardingTransaction BeginTransaction(IsolationLevel isolationLevel=IsolationLevel.Unspecified);
    }
}
