using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 09:57:08
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分表分库的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractShardingDbContext<T> : DbContext, IShardingDbContext where T : DbContext
    {
        private readonly string EMPTY_SHARDING_TAIL_ID = Guid.NewGuid().ToString("n");
        private readonly ConcurrentDictionary<string, DbContext> _dbContextCaches = new ConcurrentDictionary<string, DbContext>();
        private readonly IVirtualTableManager _virtualTableManager;
        private readonly IShardingDbContextFactory _shardingDbContextFactory;
        public AbstractShardingDbContext(DbContextOptions options) : base(options)
        {
            _shardingDbContextFactory = ShardingContainer.GetService<IShardingDbContextFactory>();
        }
        public DbContext GetDbContext(bool track, string tail)
        {
            if (!_dbContextCaches.TryGetValue(tail, out var dbContext))
            {
                dbContext = _shardingDbContextFactory.Create(track ? this.Database.GetDbConnection() : null, tail == EMPTY_SHARDING_TAIL_ID ? string.Empty : tail); ;
                _dbContextCaches.TryAdd(tail, dbContext);
            }

            //if (IsOpenTransaction)
            //{
            //    _dbTransaction.Use(dbContext);
            //}

            return dbContext;
        }

        public bool IsBeginTransaction => Database.CurrentTransaction != null;
        public DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            var tail = EMPTY_SHARDING_TAIL_ID;
            if (entity.IsShardingTable())
            {
                var physicTable = _virtualTableManager.GetVirtualTable(entity.GetType()).RouteTo(new TableRouteConfig(null, entity as IShardingTable, null))[0];
                tail = physicTable.Tail;
            }

            return GetDbContext(true, tail);
        }

        public override EntityEntry Add(object entity)
        {
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override void AddRange(params object[] entities)
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
        }

        public override void AddRange(IEnumerable<object> entities)
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
        }

        public override async Task AddRangeAsync(params object[] entities)
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
        }

        public override async Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new CancellationToken())
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
        }

        public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override EntityEntry Attach(object entity)
        {
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override void AttachRange(params object[] entities)
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
                group.Key.AttachRange(group.Select(o => o.Entity));
            }
        }

        public override void AttachRange(IEnumerable<object> entities)
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
                group.Key.AttachRange(group.Select(o => o.Entity));
            }
        }


        public override DatabaseFacade Database => _dbContextCaches.Any()
            ? _dbContextCaches.First().Value.Database
            : GetDbContext(true, string.Empty).Database;

        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry Entry(object entity)
        {
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override void UpdateRange(params object[] entities)
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
        }

        public override void UpdateRange(IEnumerable<object> entities)
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
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override EntityEntry Remove(object entity)
        {
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override void RemoveRange(params object[] entities)
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
        }

        public override void RemoveRange(IEnumerable<object> entities)
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
        }

        public override int SaveChanges()
        {
            var isBeginTransaction = IsBeginTransaction;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                Database.BeginTransaction();
            }
            int i = 0;

            try
            {

                foreach (var dbContextCache in _dbContextCaches)
                {
                    dbContextCache.Value.Database.UseTransaction(Database.CurrentTransaction.GetDbTransaction());
                    i += dbContextCache.Value.SaveChanges();
                }
                if (!isBeginTransaction)
                    Database.CurrentTransaction.Commit();
            }
            finally
            {
                if (!isBeginTransaction)
                    Database.CurrentTransaction.Dispose();
            }
            return i;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {

            var isBeginTransaction = IsBeginTransaction;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                Database.BeginTransaction();
            }
            int i = 0;

            try
            {

                foreach (var dbContextCache in _dbContextCaches)
                {
                    dbContextCache.Value.Database.UseTransaction(Database.CurrentTransaction.GetDbTransaction());
                    i += dbContextCache.Value.SaveChanges(acceptAllChangesOnSuccess);
                }
                if (!isBeginTransaction)
                    Database.CurrentTransaction.Commit();
            }
            finally
            {
                if (!isBeginTransaction)
                    Database.CurrentTransaction.Dispose();
            }
            return i;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            var isBeginTransaction = IsBeginTransaction;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                await Database.BeginTransactionAsync(cancellationToken);
            }
            int i = 0;

            try
            {

                foreach (var dbContextCache in _dbContextCaches)
                {
                    await dbContextCache.Value.Database.UseTransactionAsync(Database.CurrentTransaction.GetDbTransaction(), cancellationToken: cancellationToken);
                    i +=await dbContextCache.Value.SaveChangesAsync(cancellationToken);
                }
                if (!isBeginTransaction)
                    await Database.CurrentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (!isBeginTransaction)
                    await Database.CurrentTransaction.DisposeAsync();
            }
            return i;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {

            var isBeginTransaction = IsBeginTransaction;
            //如果是内部开的事务就内部自己消化
            if (!isBeginTransaction)
            {
                await Database.BeginTransactionAsync(cancellationToken);
            }
            int i = 0;

            try
            {

                foreach (var dbContextCache in _dbContextCaches)
                {
                    await dbContextCache.Value.Database.UseTransactionAsync(Database.CurrentTransaction.GetDbTransaction(), cancellationToken: cancellationToken);
                    i += await dbContextCache.Value.SaveChangesAsync(acceptAllChangesOnSuccess,cancellationToken);
                }
                if (!isBeginTransaction)
                    await Database.CurrentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (!isBeginTransaction)
                    await Database.CurrentTransaction.DisposeAsync();
            }
            return i;
        }

        public override void Dispose()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    dbContextCache.Value.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            foreach (var dbContextCache in _dbContextCaches)
            {
                try
                {
                    await dbContextCache.Value.DisposeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            await base.DisposeAsync();
        }

    }
}