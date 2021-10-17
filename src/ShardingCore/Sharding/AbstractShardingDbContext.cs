using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using ShardingCore.Sharding.ShardingTransactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.EFCores.OptionsExtensions;

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
    public abstract class AbstractShardingDbContext : DbContext, IShardingDbContext, ISupportShardingTransaction, ISupportShardingReadWrite
    {
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;


        public AbstractShardingDbContext(DbContextOptions options) : base(options)
        {
            var wrapOptionsExtension = options.FindExtension<ShardingWrapOptionsExtension>();
            if (wrapOptionsExtension != null)
            {
                _shardingDbContextExecutor =
                    (IShardingDbContextExecutor)Activator.CreateInstance(
                        typeof(ShardingDbContextExecutor<>).GetGenericType0(this.GetType()));
            }
        }
        /// <summary>
        /// 读写分离优先级
        /// </summary>
        public int ReadWriteSeparationPriority
        {
            get => _shardingDbContextExecutor.ReadWriteSeparationPriority;
            set => _shardingDbContextExecutor.ReadWriteSeparationPriority = value;
        }
        /// <summary>
        /// 是否使用读写分离
        /// </summary>
        public bool ReadWriteSeparation
        {
            get => _shardingDbContextExecutor.ReadWriteSeparation;
            set => _shardingDbContextExecutor.ReadWriteSeparation = value;
        }

        /// <summary>
        /// 是否是真正的执行者
        /// </summary>
        public  bool IsExecutor => _shardingDbContextExecutor == null;

        //public void ShardingUpgrade()
        //{
        //    //IsExecutor = true;
        //}

        public DbContext GetDbContext(string dataSourceName, bool parallelQuery, IRouteTail routeTail)
        {
            return _shardingDbContextExecutor.CreateDbContext(parallelQuery, dataSourceName, routeTail);
        }

        /// <summary>
        /// 根据对象创建通用的dbcontext
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            return _shardingDbContextExecutor.CreateGenericDbContext(entity);
        }


        public override EntityEntry Add(object entity)
        {
            if (IsExecutor)
                base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            if (IsExecutor)
                return base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }



#if !EFCORE2

        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }
#endif
#if EFCORE2
        public override Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override Task<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }
#endif

        public override void AddRange(params object[] entities)
        {
            if (IsExecutor)
            {
                base.AddRange(entities);
                return;
            }
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
            if (IsExecutor)
            {
                base.AddRange(entities);
                return;
            }
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
            if (IsExecutor)
            {
                await base.AddRangeAsync(entities);
                return;
            }
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
            if (IsExecutor)
            {
                await base.AddRangeAsync(entities, cancellationToken);
                return;
            }
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
            if (IsExecutor)
                return base.Attach(entity);
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override EntityEntry Attach(object entity)
        {
            if (IsExecutor)
                return base.Attach(entity);
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override void AttachRange(params object[] entities)
        {
            if (IsExecutor)
            {
                base.AttachRange(entities);
                return;
            }
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
            if (IsExecutor)
            {
                base.AttachRange(entities);
                return;
            }
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


        //public override DatabaseFacade Database => _dbContextCaches.Any()
        //    ? _dbContextCaches.First().Value.Database
        //    : GetDbContext(true, string.Empty).Database;

        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        {
            if (IsExecutor)
                return base.Entry(entity);
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry Entry(object entity)
        {
            if (IsExecutor)
                return base.Entry(entity);
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            if (IsExecutor)
                return base.Update(entity);
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            if (IsExecutor)
                return base.Update(entity);
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override void UpdateRange(params object[] entities)
        {
            if (IsExecutor)
            {
                base.UpdateRange(entities);
                return;
            }
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
            if (IsExecutor)
            {
                base.UpdateRange(entities);
                return;
            }
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
            if (IsExecutor)
                return base.Remove(entity);
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override EntityEntry Remove(object entity)
        {
            if (IsExecutor)
                return base.Remove(entity);
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override void RemoveRange(params object[] entities)
        {
            if (IsExecutor)
            {
                base.RemoveRange(entities);
                return;
            }
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
            if (IsExecutor)
            {
                base.RemoveRange(entities);
                return;
            }
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

        //protected virtual void ApplyShardingConcepts()
        //{
        //    foreach (var entry in ChangeTracker.Entries().ToList())
        //    {
        //        ApplyShardingConcepts(entry);
        //    }
        //}

        //protected virtual void ApplyShardingConcepts(EntityEntry entry)
        //{

        //    switch (entry.State)
        //    {
        //        case EntityState.Added:
        //        case EntityState.Modified:
        //        case EntityState.Deleted:
        //            ApplyShardingConceptsForEntity(entry);
        //            break;
        //    }

        //    //throw new ShardingCoreNotSupportedException($"entry.State:[{entry.State}]");
        //}

        //protected virtual void ApplyShardingConceptsForEntity(EntityEntry entry)
        //{
        //    var genericDbContext = CreateGenericDbContext(entry.Entity);
        //    var entityState = entry.State;
        //    entry.State = EntityState.Unchanged;
        //    genericDbContext.Entry(entry.Entity).State = entityState;
        //}
        public override int SaveChanges()
        {

            if (IsExecutor)
                return base.SaveChanges();
            return this.SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (IsExecutor)
                return base.SaveChanges(acceptAllChangesOnSuccess);
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (!_shardingDbContextExecutor.IsBeginTransaction)
            {
                using (var tran = _shardingDbContextExecutor.BeginTransaction())
                {
                    i = _shardingDbContextExecutor.SaveChanges(acceptAllChangesOnSuccess);
                    tran.Commit();
                }
            }
            else
            {
                i = _shardingDbContextExecutor.SaveChanges(acceptAllChangesOnSuccess);
            }

            return i;
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return base.SaveChangesAsync(cancellationToken);
            return this.SaveChangesAsync(true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess,cancellationToken);
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (!_shardingDbContextExecutor.IsBeginTransaction)
            {
                using (var tran = _shardingDbContextExecutor.BeginTransaction())
                {
                    i = await _shardingDbContextExecutor.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
#if EFCORE2
                     tran.Commit();
#endif
#if !EFCORE2
                    await tran.CommitAsync(cancellationToken);
#endif
                }
            }
            else
            {
                i = await _shardingDbContextExecutor.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }


            return i;
        }

        public override void Dispose()
        {

            if (IsExecutor)
            {
                base.Dispose();
            }
            else
            {
                _shardingDbContextExecutor.Dispose();
                base.Dispose();
            }
        }
#if !EFCORE2

        public override async ValueTask DisposeAsync()
        {
            if (IsExecutor)
            {
                await base.DisposeAsync();
            }
            else
            {
                await _shardingDbContextExecutor.DisposeAsync();

                await base.DisposeAsync();
            }
        }
#endif

        public IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            return _shardingDbContextExecutor.BeginTransaction(isolationLevel);
        }
    }
}