using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace ShardingCore.Sharding.Abstractions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/18 13:30:08
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    [Obsolete("can remove")]
    public interface ISupportShardingTransaction
    {
//        void NotifyShardingTransaction();
//        void Rollback();
//        void Commit();
//#if !EFCORE2
//        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
//        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
//#endif
    }
}
