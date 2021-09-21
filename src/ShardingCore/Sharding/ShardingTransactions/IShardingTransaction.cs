using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.ShardingTransactions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 8:41:50
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public interface IShardingTransaction:IDisposable
#if !EFCORE2
,IAsyncDisposable
#endif
    {
        bool IsBeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        void Use(string dataSourceName,DbContext dbContext);
        void Rollback();
        void Commit();
#if !EFCORE2
        Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken());
        Task CommitAsync(CancellationToken cancellationToken = new CancellationToken());
#endif
    }
}
