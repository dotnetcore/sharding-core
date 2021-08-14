using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.Abstractions;
using ShardingCore.DbContexts.Transactions;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 13 August 2021 23:26:43
* @Email: 326308290@qq.com
*/
    public class ShardingCruder : IShardingCruder
    {
        private readonly DbContext _dbContext;
        private readonly string EMPTY_SHARDING_TAIL_ID = Guid.NewGuid().ToString("n");
        private readonly ConcurrentDictionary<string, DbContext> _dbContextCaches = new ConcurrentDictionary<string, DbContext>();
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private IShardingTransaction _dbTransaction;
        public bool IsOpenTransaction => _dbTransaction != null && _dbTransaction.IsOpened;

        public ShardingCruder(DbContext dbContext)
        {
            _dbContext = dbContext;
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager>();
            _shardingDbContextFactory = ShardingContainer.GetService<IShardingDbContextFactory>();
        }

        public IShardingTransaction BeginShardingTransaction()
        {
            if (IsOpenTransaction)
                return _dbTransaction;
            _dbTransaction = new ShardingTransaction();
            _dbTransaction.Open();
            _dbContextCaches.ForEach(kv => { _dbTransaction.Use(kv.Value); });
            return _dbTransaction;
        }

        public void Rollback()
        {
            _dbTransaction?.Rollback();
        }

        public async Task RollbackAsync()
        {
            if (_dbTransaction != null)
                await _dbTransaction.RollbackAsync();
        }


        public async Task<int> InsertAsync<T>(T entity) where T : class
        {
            await CreateGenericDbContext(entity).Set<T>().AddAsync(entity);
            return 1;
        }

        public async Task<int> InsertRangeAsync<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                await group.Key.AddRangeAsync(group.Select(o => o.Entity));
            }

            return entities.Count;
        }

        public Task<int> UpdateAsync<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Update(entity);
            return Task.FromResult(1);
        }

        public Task<int> UpdateRangeAsync<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.UpdateRange(group.Select(o => o.Entity));
            }

            return Task.FromResult(entities.Count);
        }

        public Task<int> DeleteAsync<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Remove(entity);
            return Task.FromResult(1);
        }

        public Task<int> DeleteRangeAsync<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.RemoveRange(group.Select(o => o.Entity));
            }

            return Task.FromResult(entities.Count);
        }

        public int Insert<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Add(entity);
            return 1;
        }

        public int InsertRange<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.AddRange(group.Select(o => o.Entity));
            }

            return entities.Count;
        }

        public int Update<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Update(entity);
            return 1;
        }


        public void UpdateColumns<T>(T entity, Expression<Func<T, object>> getUpdatePropertyNames) where T : class
        {
            var context = CreateGenericDbContext(entity);
            context.Set<T>().Attach(entity);
            var props = GetUpdatePropNames(entity, getUpdatePropertyNames);
            foreach (var prop in props)
            {
                context.Entry(entity).Property(prop).IsModified = true;
            }
        }

        private IEnumerable<string> GetUpdatePropNames<T>(T entity, Expression<Func<T, object>> getUpdatePropertyNames) where T : class
        {
            var updatePropertyNames = getUpdatePropertyNames.Compile()(entity);
            var fullPropNames = entity.GetType().GetProperties().Select(o => o.Name);
            var updatePropNames = updatePropertyNames.GetType().GetProperties().Select(o => o.Name);
            return updatePropNames.Intersect(fullPropNames);
        }

        public void UpdateWithOutIgnoreColumns<T>(T entity, Expression<Func<T, object>> getIgnorePropertyNames) where T : class
        {
            var context = CreateGenericDbContext(entity);
            context.Entry(entity).State = EntityState.Modified;
            var props = GetIgnorePropNames(entity, getIgnorePropertyNames);
            foreach (var prop in props)
            {
                context.Entry(entity).Property(prop).IsModified = false;
            }
        }

        private IEnumerable<string> GetIgnorePropNames<T>(T entity, Expression<Func<T, object>> getIgnorePropertyNames) where T : class
        {
            var ignoreProp = getIgnorePropertyNames.Compile()(entity);
            var fullUpdatePropNames = entity.GetType().GetProperties().Select(o => o.Name);
            var ignorePropNames = ignoreProp.GetType().GetProperties().Select(o => o.Name);
            return ignorePropNames.Intersect(fullUpdatePropNames);
        }

        public int UpdateRange<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.UpdateRange(group.Select(o => o.Entity));
            }

            return entities.Count;
        }

        public int Delete<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Remove(entity);
            return 1;
        }

        public int DeleteRange<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
            {
                var dbContext = CreateGenericDbContext(o);
                return new
                {
                    DbContext = dbContext,
                    Entity = o
                };
            }).GroupBy(g => g.DbContext);

            foreach (var group in groups)
            {
                group.Key.RemoveRange(group.Select(o => o.Entity));
            }

            return entities.Count;
        }

        private DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            var tail = EMPTY_SHARDING_TAIL_ID;
            if (entity.IsShardingTable())
            {
                var physicTable = _virtualTableManager.GetVirtualTable(entity.GetType()).RouteTo(new TableRouteConfig(null, entity as IShardingTable, null))[0];
                tail = physicTable.Tail;
            }

            return GetOrCreateShardingDbContext(tail);
        }

        private DbContext GetOrCreateShardingDbContext(string tail)
        {
            if (!_dbContextCaches.TryGetValue(tail, out var dbContext))
            {
                if (tail == EMPTY_SHARDING_TAIL_ID)
                {
                    _dbContextCaches.TryAdd(tail, _dbContext);
                }
                else
                {
                    dbContext = _shardingDbContextFactory.Create(tail, false);
                    _dbContextCaches.TryAdd(tail, dbContext);
                }
            }

            if (IsOpenTransaction)
            {
                _dbTransaction.Use(dbContext);
            }

            return dbContext;
        }


        public int SaveChanges()
        {
            try
            {
                if (_dbContextCaches.IsEmpty())
                    return 0;
                var needOpenTran = _dbContextCaches.Count>1;
                if (needOpenTran)
                {
                    BeginShardingTransaction();
                }

                var effects = 0;
                foreach (var dbContextCache in _dbContextCaches)
                {
                    effects += dbContextCache.Value.SaveChanges();
                }

                if (IsOpenTransaction)
                    _dbTransaction.Commit();

                return effects;
            }
            finally
            {
                Clear();
            }
        }


        public async Task<int> SaveChangesAsync()
        {
            try
            {
                if (_dbContextCaches.IsEmpty())
                    return 0;
                var needOpenTran = _dbContextCaches.Count>1;
                if (needOpenTran)
                {
                    BeginShardingTransaction();
                }

                var effects = 0;
                foreach (var dbContextCache in _dbContextCaches)
                {
                    effects += await dbContextCache.Value.SaveChangesAsync();
                }

                if (IsOpenTransaction)
                    await _dbTransaction.CommitAsync();

                return effects;
            }
            finally
            {
                Clear();
            }
        }

        private void Clear()
        {
            
            _dbTransaction?.Dispose();
            _dbTransaction = null;
            try
            {
                _dbContextCaches.SkipWhile(o => o.Key == EMPTY_SHARDING_TAIL_ID)
                    .ForEach(o =>
                    {
                        try
                        {
                            o.Value.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                _dbContextCaches.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }
}