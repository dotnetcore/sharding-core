using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ShardingCore.DbContexts.Transactions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 04:19:53
* @Email: 326308290@qq.com
*/
    public interface IShardingTransaction:IDisposable
    {
        bool IsOpened { get; }
        bool IsUsed { get; }
        void Use(DbContext dbContext);
        void Open();
        void Rollback();
        Task RollbackAsync();
        void Commit();
        Task CommitAsync();
        IDbContextTransaction GetDbContextTransaction();
    }
}
