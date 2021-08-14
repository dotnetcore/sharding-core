using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShardingCore.DbContexts.Transactions;

namespace ShardingCore.DbContexts.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 13 August 2021 22:46:59
* @Email: 326308290@qq.com
*/
    public interface IShardingCruder:IDisposable
    {
        IShardingTransaction BeginShardingTransaction();
        void Rollback();
        Task RollbackAsync();
        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> InsertRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> UpdateRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
        Task<int> DeleteRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> SaveChangesAsync();
        int Insert<T>(T entity) where T : class;
        int InsertRange<T>(ICollection<T> entities) where T : class;
        int Update<T>(T entity) where T : class;int UpdateRange<T>(ICollection<T> entities) where T : class;
        int Delete<T>(T entity) where T : class;
        int DeleteRange<T>(ICollection<T> entities) where T : class;
        int SaveChanges();
        
        
        
        void UpdateColumns<T>(T entity,Expression<Func<T,object>> getUpdatePropertyNames) where T : class;
        void UpdateWithOutIgnoreColumns<T>(T entity,Expression<Func<T,object>> getIgnorePropertyNames) where T : class;

    }
}