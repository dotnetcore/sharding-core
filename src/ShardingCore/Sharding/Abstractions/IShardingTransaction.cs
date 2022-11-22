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
#if !EFCORE3 && !NETSTANDARD2_0
        void CreateSavepoint(string name);
        Task CreateSavepointAsync(string name, CancellationToken cancellationToken = new CancellationToken());
        void RollbackToSavepoint(string name);
        Task RollbackToSavepointAsync(string name,CancellationToken cancellationToken = default(CancellationToken));
        void ReleaseSavepoint(string name);
        Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default(CancellationToken));
#endif
#endif
    }
}