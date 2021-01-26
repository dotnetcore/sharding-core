using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShardingCore.DbContexts.Transactions;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
    /**
	 * 描述：IVirtualDbContext
	 * 
	 * Author：xuejiaming
	 * Created: 2020/6/15 11:42:52
	 **/
    public interface IVirtualDbContext:IDisposable
    {
        IShardingTransaction BeginTransaction();
        void Rollback();
        Task RollbackAsync();
        IQueryable<T> Set<T>() where T : class;
        Task<int> InsertAsync<T>(T entity) where T : class;
        Task<int> InsertRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> UpdateAsync<T>(T entity) where T : class;
        Task<int> UpdateRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> DeleteAsync<T>(T entity) where T : class;
        Task<int> DeleteRangeAsync<T>(ICollection<T> entities) where T : class;
        Task<int> SaveChangesAsync();
        int Insert<T>(T entity) where T : class;
        int InsertRange<T>(ICollection<T> entities) where T : class;
        int Update<T>(T entity) where T : class;
        int UpdateRange<T>(ICollection<T> entities) where T : class;
        int Delete<T>(T entity) where T : class;
        int DeleteRange<T>(ICollection<T> entities) where T : class;
        int SaveChanges();
        
        
        #region 批处理

        /// <summary>
        /// 批量插入直接提交
        /// </summary>
        /// <param name="entities"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ShardingBatchInsertEntry<T> BulkInsert<T>(ICollection<T> entities) where T : class;
        /// <summary>
        /// 批量更新直接提交
        /// </summary>
        /// <param name="where"></param>
        /// <param name="updateExp"></param>
        /// <typeparam name="T"></typeparam>
        ShardingBatchUpdateEntry<T> BulkUpdate<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> updateExp) where T : class;
       
        /// <summary>
        /// 批量删除直接提交
        /// </summary>
        /// <param name="where"></param>
        /// <typeparam name="T"></typeparam>
        ShardingBatchDeleteEntry<T> BulkDelete<T>(Expression<Func<T, bool>> where) where T : class;

        #endregion
    }
}