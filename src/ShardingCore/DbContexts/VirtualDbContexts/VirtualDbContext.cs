using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
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
        private readonly IVirtualDataSourceManager _virtualDataSourceManager;
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DbContext>> _dbContextCaches = new ConcurrentDictionary<string,ConcurrentDictionary<string, DbContext>>();

        private IShardingTransaction _dbTransaction = new ShardingTransaction();

        public VirtualDbContext(IServiceProvider serviceProvider, IVirtualDataSourceManager virtualDataSourceManager,
            IVirtualTableManager virtualTableManager, IShardingDbContextFactory shardingDbContextFactory) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _virtualDataSourceManager = virtualDataSourceManager;
            _virtualTableManager = virtualTableManager;
            _shardingDbContextFactory = shardingDbContextFactory;
        }

        private bool IsOpenTransaction => _dbTransaction.IsOpened;


        public IShardingTransaction BeginTransaction()
        {
            _dbTransaction.Open();
            _dbContextCaches.ForEach(kv =>
            {
                kv.Value.ForEach(d =>
                {
                    _dbTransaction.Use(d.Value);
                });
            });
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
            return GetOrCreateShardingDbContext(_virtualDataSourceManager.GetDefaultConnectKey(),EMPTY_SHARDING_TAIL_ID).Set<T>().AsNoTracking();
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

        public async Task<int> SaveChangesAsync()
        {
            var transOpenNow = !_dbTransaction.IsOpened && _dbContextCaches.Count > 1;
            if (transOpenNow)
            {
                BeginTransaction();
            }
            var effects = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var dbContext in dbContextCache.Value)
                {
                    effects += await dbContext.Value.SaveChangesAsync();
                }
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
                group.Key.AddRange(group.Select(o => o.Entity));
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

        public int SaveChanges()
        {
            var transOpenNow = !_dbTransaction.IsOpened && _dbContextCaches.Count > 1;
            if (transOpenNow)
            {
                BeginTransaction();
            }
            var effects = 0;
            foreach (var dbContextCache in _dbContextCaches)
            {
                foreach (var dbContext in dbContextCache.Value)
                {
                    effects += dbContext.Value.SaveChanges();
                }
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
                .ToDictionary(o => (DbContext)o.Key, o => o.Select(item => item.Entity).ToList());
            return new ShardingBatchInsertEntry<T>(groups);
        }

        public ShardingBatchUpdateEntry<T> BulkUpdate<T>(Expression<Func<T, bool>> where, Expression<Func<T, T>> updateExp) where T : class
        {
            List<(string connectKey, List<DbContext> dbContexts)> dbContexts = null;
            var connectKeys = _virtualDataSourceManager.GetConnectKeys<T>(where);

            if (typeof(T).IsShardingTable())
            {
                dbContexts = CreateShardingDbContexts<IShardingTable>(connectKeys, new EnumerableQuery<T>(where).AsQueryable());
            }
            else
            {
                dbContexts = connectKeys.Select(connectKey =>
                {
                    return (connectKey, new List<DbContext>(1)
                    {
                        GetOrCreateShardingDbContext(connectKey, EMPTY_SHARDING_TAIL_ID)
                    });
                }).ToList();
            }

            return new ShardingBatchUpdateEntry<T>(where, updateExp, dbContexts);
        }

        public ShardingBatchDeleteEntry<T> BulkDelete<T>(Expression<Func<T, bool>> where) where T : class
        {
            List<(string connectKey,List<DbContext> dbContexts)> dbContexts = null;
            var connectKeys = _virtualDataSourceManager.GetConnectKeys<T>(where);

            if (typeof(T).IsShardingTable())
            {
                dbContexts = CreateShardingDbContexts<IShardingTable>(connectKeys, new EnumerableQuery<T>(where).AsQueryable());
            }
            else
            {
                dbContexts = connectKeys.Select(connectKey =>
                {
                    return (connectKey, new List<DbContext>(1)
                    {
                        GetOrCreateShardingDbContext(connectKey, EMPTY_SHARDING_TAIL_ID)
                    });
                }).ToList();
            }

            return new ShardingBatchDeleteEntry<T>(where, dbContexts);
        }

        private DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            var tail = EMPTY_SHARDING_TAIL_ID;
            var connectKey = _virtualDataSourceManager.GetConnectKey(entity);
            if (entity.IsShardingTable())
            {
                var physicTable = _virtualTableManager.GetVirtualTable(connectKey, entity.GetType()).RouteTo(new TableRouteConfig(null, entity as IShardingTable, null))[0];
                tail = physicTable.Tail;
            }
            return GetOrCreateShardingDbContext(connectKey,tail);
        }

        private List<(string connectKey, List<DbContext> dbContexts)> CreateShardingDbContexts<T>(List<string> connectKeys,IQueryable queryable) where T : class, IShardingTable
        {
            var results =
                new List<(string connectKey, List<DbContext> dbContexts)>();
            foreach (var connectKey in connectKeys)
            {
                var physicTables = _virtualTableManager.GetVirtualTable(connectKey,typeof(T)).RouteTo(new TableRouteConfig(queryable, null, null));
                if (physicTables.Any())
                {
                    var dbContexts = new List<DbContext>(physicTables.Count);
                    foreach (var physicTable in physicTables)
                    {
                        var dbContext = GetOrCreateShardingDbContext(connectKey,physicTable.Tail);

                        dbContexts.Add(dbContext);
                    }
                    results.Add((connectKey, dbContexts));
                }
            }

            if (results.Any())
                return results;

            throw new QueryableRouteNotMatchException($"{typeof(T)} -> {nameof(queryable)}");
        }

        private DbContext GetOrCreateShardingDbContext(string connectKey,string tail)
        {
            if (!_dbContextCaches.TryGetValue(connectKey, out var dbContexts))
            {
                dbContexts = new ConcurrentDictionary<string, DbContext>();
                _dbContextCaches.TryAdd(connectKey, dbContexts);
            }
            if(!dbContexts.TryGetValue(tail,out var dbContext))
            {
                dbContext = _shardingDbContextFactory.Create(connectKey, tail == EMPTY_SHARDING_TAIL_ID ? string.Empty : tail, _serviceProvider.GetService<IDbContextOptionsProvider>());
                dbContexts.TryAdd(tail, dbContext);
            }

            if (IsOpenTransaction)
            {
                _dbTransaction.Use(dbContext);
            }

            return dbContext;
            //if (!_dbContextCaches.TryGetValue(tail, out var dbContext))
            //{
            //    var virtualTableConfigs = _virtualTableManager.GetAllVirtualTables().GetVirtualTableDbContextConfigs();
            //    dbContext = _shardingDbContextFactory.Create(new ShardingDbContextOptions(DbContextOptionsProvider.GetDbContextOptions(), tail == EMPTY_SHARDING_TAIL_ID ? string.Empty : tail, virtualTableConfigs));
            //    _dbContextCaches.TryAdd(tail, dbContext);
            //}

            //if (IsOpenTransaction)
            //{
            //    _dbTransaction.Use(dbContext);
            //}

            //return dbContext;
        }


        public void Dispose()
        {
            try
            {
                _dbContextCaches.ForEach(o =>
                {
                    o.Value.ForEach(v =>
                    {
                        try
                        {
                            v.Value.Dispose();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
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