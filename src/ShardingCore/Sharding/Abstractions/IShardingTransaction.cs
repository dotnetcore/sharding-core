using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/3/30 14:09:29
    /// Email: 326308290@qq.com
    public interface IShardingTransaction
    {
        void NotifyShardingTransaction();
        void Rollback();
        void Commit();
#if !EFCORE2
        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
#endif
    }
}
