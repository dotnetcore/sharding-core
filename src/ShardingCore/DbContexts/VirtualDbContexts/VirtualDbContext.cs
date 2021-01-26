using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.Transactions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.DbContexts.VirtualDbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 17 December 2020 21:50:49
* @Email: 326308290@qq.com
*/
    public class VirtualDbContext : AbstractInjectVirtualDbContext, IVirtualDbContext
    {
        private readonly string EMPTY_SHARDING_TAIL_ID = Guid.NewGuid().ToString("n");
        private readonly IServiceProvider _serviceProvider;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly ConcurrentDictionary<string, DbContext> _dbContextCaches = new ConcurrentDictionary<string, DbContext>();

        private IShardingTransaction _dbTransaction = new ShardingTransaction();

        public VirtualDbContext(IServiceProvider serviceProvider,
            IVirtualTableManager virtualTableManager, IShardingDbContextFactory shardingDbContextFactory) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _virtualTableManager = virtualTableManager;
            _shardingDbContextFactory = shardingDbContextFactory;
        }

        private bool IsOpenTransaction => _dbTransaction.IsOpened;


        public IShardingTransaction BeginTransaction()
        {
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

        public IQueryable<T> Set<T>() where T : class
        {
            return GetOrCreateShardingDbContext(EMPTY_SHARDING_TAIL_ID).Set<T>().AsNoTracking();
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
                await group.Key.AddRangeAsync(group.Select(o=>o.Entity));
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
                group.Key.UpdateRange(group.Select(o=>o.Entity));
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
                group.Key.RemoveRange(group.Select(o=>o.Entity));
            }

            return Task.FromResult(entities.Count);
        }

        public async Task<int> SaveChangesAsync()
        {
            var transOpenNow = !_dbTransaction.IsOpened&&_dbContextCaches.Count>1;
            if (transOpenNow)
            {
                BeginTransaction();
            }
            var effects = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                effects += await dbContextCache.Value.SaveChangesAsync();
            }

            if (transOpenNow)
                await _dbTransaction.CommitAsync();

            return effects;
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
                group.Key.AddRange(group.Select(o=>o.Entity));
            }

            return entities.Count;
        }

        public int Update<T>(T entity) where T : class
        {
            CreateGenericDbContext(entity).Set<T>().Update(entity);
            return 1;
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
                group.Key.UpdateRange(group.Select(o=>o.Entity));
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
                group.Key.RemoveRange(group.Select(o=>o.Entity));
            }

            return entities.Count;
        }

        public int SaveChanges()
        {
            var transOpenNow = !_dbTransaction.IsOpened&&_dbContextCaches.Count>1;
            if (transOpenNow)
            {
                BeginTransaction();
            }
            var effects = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                effects += dbContextCache.Value.SaveChanges();
            }
            if (transOpenNow)
               _dbTransaction.Commit();

            return effects;
        }

        public ShardingBatchInsertEntry<T> BulkInsert<T>(ICollection<T> entities) where T : class
        {
            var groups = entities.Select(o =>
                {
                    var dbContext = CreateGenericDbContext(o);
                    return new
                    {
                        DbContext = dbContext,
                        Entity = o
                    };
                }).GroupBy(g => g.DbContext)
                .ToDictionary(o => (DbContext) o.Key, o => o.Select(item => item.Entity).ToList());
            return new ShardingBatchInsertEntry<T>(groups);
        }

        public ShardingBatchUpdateEntry<T> BulkUpdate<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp) where T : class
        {
            List<DbContext> dbContexts = null;
            if (typeof(T).IsShardingEntity())
            {
                var shardingDbContexts = CreateShardingDbContext<IShardingEntity>(new EnumerableQuery<T>(where).AsQueryable());
                dbContexts = shardingDbContexts.Select(o => (DbContext) o).ToList();
            }
            else
            {
                var dbContext = CreateNoShardingDbContext();
                dbContexts = new List<DbContext>(1)
                {
                    dbContext
                };
            }

            return new ShardingBatchUpdateEntry<T>(@where,updateExp,dbContexts);
        }

        public ShardingBatchDeleteEntry<T> BulkDelete<T>(Expression<Func<T, bool>> @where) where T : class
        {
            List<DbContext> dbContexts = null;
            if (typeof(T).IsShardingEntity())
            {
                var shardingDbContexts = CreateShardingDbContext<IShardingEntity>(new EnumerableQuery<T>(where).AsQueryable());
                dbContexts = shardingDbContexts.Select(o => (DbContext) o).ToList();
            }
            else
            {
                var dbContext = CreateNoShardingDbContext();
                dbContexts = new List<DbContext>(1)
                {
                    dbContext
                };
            }

            return new ShardingBatchDeleteEntry<T>(@where,dbContexts);
        }

        private ShardingDbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            if (entity.IsShardingEntity())
            {
                return CreateShardingDbContext(entity as IShardingEntity);
            }
            else
            {
                return CreateNoShardingDbContext();
            }
        }

        private ShardingDbContext CreateNoShardingDbContext()
        {
            var shardingDbContext = GetOrCreateShardingDbContext(EMPTY_SHARDING_TAIL_ID);

            return shardingDbContext;
        }

        private ShardingDbContext CreateShardingDbContext<T>(T entity) where T : class, IShardingEntity
        {
            var physicTable = _virtualTableManager.GetVirtualTable(entity.GetType()).RouteTo(new RouteConfig(null, entity, null))[0];
            var shardingDbContext = GetOrCreateShardingDbContext(physicTable.Tail);

            return shardingDbContext;
        }

        private List<ShardingDbContext> CreateShardingDbContext<T>(IQueryable queryable) where T : class, IShardingEntity
        {
            var physicTables = _virtualTableManager.GetVirtualTable(typeof(T)).RouteTo(new RouteConfig(queryable, null, null));
            if (physicTables.Any())
            {
                var shardingDbContexts = new List<ShardingDbContext>(physicTables.Count);
                foreach (var physicTable in physicTables)
                {
                    var shardingDbContext = GetOrCreateShardingDbContext(physicTable.Tail);

                    shardingDbContexts.Add(shardingDbContext);
                }

                return shardingDbContexts;
            }

            throw new QueryableRouteNotMatchException($"{typeof(T)} -> {nameof(queryable)}");
        }

        private ShardingDbContext GetOrCreateShardingDbContext(string tail)
        {
            if (!_dbContextCaches.TryGetValue(tail, out var shardingDbContext))
            {
                var virtualTableConfigs = _virtualTableManager.GetAllVirtualTables().GetVirtualTableDbContextConfigs();
                shardingDbContext = _shardingDbContextFactory.Create(new ShardingDbContextOptions(DbContextOptionsProvider.GetDbContextOptions(), tail == EMPTY_SHARDING_TAIL_ID ? string.Empty : tail, virtualTableConfigs));
                _dbContextCaches.TryAdd(tail, shardingDbContext);
            }

            if (IsOpenTransaction)
            {
                _dbTransaction.Use(shardingDbContext);
            }

            return (ShardingDbContext) shardingDbContext;
        }


        public void Dispose()
        {
            try
            {
                _dbContextCaches.ForEach(o =>
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
    }
}