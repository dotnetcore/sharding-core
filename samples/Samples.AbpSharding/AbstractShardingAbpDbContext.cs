//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Microsoft.EntityFrameworkCore.Storage;
//using ShardingCore;
//using ShardingCore.Core;
//using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
//using ShardingCore.Extensions;
//using ShardingCore.Sharding.Abstractions;
//using ShardingCore.Sharding.ShardingDbContextExecutors;
//using ShardingCore.Sharding.ShardingTransactions;
//using Volo.Abp.EntityFrameworkCore;

//namespace Samples.AbpSharding
//{
//    public abstract class AbstractShardingAbpDbContext : AbpDbContext<AbstractShardingAbpDbContext>, IShardingDbContext, ISupportShardingTransaction, ISupportShardingReadWrite
//    {
//        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;
//        protected AbstractShardingAbpDbContext(DbContextOptions<AbstractShardingAbpDbContext> options) : base(options)
//        {

//            _shardingDbContextExecutor =
//                (IShardingDbContextExecutor)Activator.CreateInstance(
//                    typeof(ShardingDbContextExecutor<>).GetGenericType0(this.GetType()));
//        }


//        /// <summary>
//        /// 读写分离优先级
//        /// </summary>
//        public int ReadWriteSeparationPriority
//        {
//            get => _shardingDbContextExecutor.ReadWriteSeparationPriority;
//            set => _shardingDbContextExecutor.ReadWriteSeparationPriority = value;
//        }
//        /// <summary>
//        /// 是否使用读写分离
//        /// </summary>
//        public bool ReadWriteSeparation
//        {
//            get => _shardingDbContextExecutor.ReadWriteSeparation;
//            set => _shardingDbContextExecutor.ReadWriteSeparation = value;
//        }

//        public new bool IsExecutor { get; private set; }

//        public void ShardingUpgrade()
//        {
//            IsExecutor = true;
//        }

//        public DbContext GetDbContext(string dataSourceName, bool parallelQuery, IRouteTail routeTail)
//        {
//            var dbContext = _shardingDbContextExecutor.CreateDbContext(parallelQuery, dataSourceName, routeTail);
//            if (!parallelQuery)
//                ((AbpDbContext<AbstractShardingAbpDbContext>)dbContext).LazyServiceProvider = this.LazyServiceProvider;
//            return dbContext;
//        }

//        /// <summary>
//        /// 根据对象创建通用的dbcontext
//        /// </summary>
//        /// <typeparam name="TEntity"></typeparam>
//        /// <param name="entity"></param>
//        /// <returns></returns>
//        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
//        {
//            return _shardingDbContextExecutor.CreateGenericDbContext(entity);
//        }


//        public override EntityEntry Add(object entity)
//        {
//            return CreateGenericDbContext(entity).Add(entity);
//        }

//        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
//        {
//            return CreateGenericDbContext(entity).Add(entity);
//        }




//        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
//        {
//            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
//        }

//        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
//        {
//            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
//        }

//        public override void AddRange(params object[] entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.AddRange(group.Select(o => o.Entity));
//            }
//        }

//        public override void AddRange(IEnumerable<object> entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.AddRange(group.Select(o => o.Entity));
//            }
//        }

//        public override async Task AddRangeAsync(params object[] entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                await group.Key.AddRangeAsync(group.Select(o => o.Entity));
//            }
//        }

//        public override async Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new CancellationToken())
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                await group.Key.AddRangeAsync(group.Select(o => o.Entity));
//            }
//        }

//        public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
//        {
//            return CreateGenericDbContext(entity).Attach(entity);
//        }

//        public override EntityEntry Attach(object entity)
//        {
//            return CreateGenericDbContext(entity).Attach(entity);
//        }

//        public override void AttachRange(params object[] entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.AttachRange(group.Select(o => o.Entity));
//            }
//        }

//        public override void AttachRange(IEnumerable<object> entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.AttachRange(group.Select(o => o.Entity));
//            }
//        }


//        //public override DatabaseFacade Database => _dbContextCaches.Any()
//        //    ? _dbContextCaches.First().Value.Database
//        //    : GetDbContext(true, string.Empty).Database;

//        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
//        {
//            return CreateGenericDbContext(entity).Entry(entity);
//        }

//        public override EntityEntry Entry(object entity)
//        {
//            return CreateGenericDbContext(entity).Entry(entity);
//        }

//        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
//        {
//            return CreateGenericDbContext(entity).Update(entity);
//        }

//        public override EntityEntry Update(object entity)
//        {
//            return CreateGenericDbContext(entity).Update(entity);
//        }

//        public override void UpdateRange(params object[] entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.UpdateRange(group.Select(o => o.Entity));
//            }
//        }

//        public override void UpdateRange(IEnumerable<object> entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.UpdateRange(group.Select(o => o.Entity));
//            }
//        }

//        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
//        {
//            return CreateGenericDbContext(entity).Remove(entity);
//        }

//        public override EntityEntry Remove(object entity)
//        {
//            return CreateGenericDbContext(entity).Remove(entity);
//        }

//        public override void RemoveRange(params object[] entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.RemoveRange(group.Select(o => o.Entity));
//            }
//        }

//        public override void RemoveRange(IEnumerable<object> entities)
//        {
//            var groups = entities.Select(o =>
//            {
//                var dbContext = CreateGenericDbContext(o);
//                return new
//                {
//                    DbContext = dbContext,
//                    Entity = o
//                };
//            }).GroupBy(g => g.DbContext);

//            foreach (var group in groups)
//            {
//                group.Key.RemoveRange(group.Select(o => o.Entity));
//            }
//        }
//        public override int SaveChanges()
//        {
//            return this.SaveChanges(true);
//        }

//        public override int SaveChanges(bool acceptAllChangesOnSuccess)
//        {
//            //ApplyShardingConcepts();
//            int i = 0;
//            //如果是内部开的事务就内部自己消化
//            if (!_shardingDbContextExecutor.IsBeginTransaction)
//            {
//                using (var tran = _shardingDbContextExecutor.BeginTransaction())
//                {
//                    i = _shardingDbContextExecutor.SaveChanges(acceptAllChangesOnSuccess);
//                    tran.Commit();
//                }
//            }
//            else
//            {
//                i = _shardingDbContextExecutor.SaveChanges(acceptAllChangesOnSuccess);
//            }

//            return i;
//        }


//        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
//        {
//            return this.SaveChangesAsync(true, cancellationToken);
//        }

//        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
//        {
//            //ApplyShardingConcepts();
//            int i = 0;
//            //如果是内部开的事务就内部自己消化
//            if (!_shardingDbContextExecutor.IsBeginTransaction)
//            {
//                using (var tran = _shardingDbContextExecutor.BeginTransaction())
//                {
//                    i = await _shardingDbContextExecutor.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

//                    await tran.CommitAsync(cancellationToken);
//                }
//            }
//            else
//            {
//                i = await _shardingDbContextExecutor.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
//            }


//            return i;
//        }

//        public override void Dispose()
//        {
//            _shardingDbContextExecutor.Dispose();
//            base.Dispose();
//        }

//        public override async ValueTask DisposeAsync()
//        {
//            await _shardingDbContextExecutor.DisposeAsync();

//            await base.DisposeAsync();
//        }

//        public IShardingTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
//        {
//            return _shardingDbContextExecutor.BeginTransaction(isolationLevel);
//        }
//    }
//}