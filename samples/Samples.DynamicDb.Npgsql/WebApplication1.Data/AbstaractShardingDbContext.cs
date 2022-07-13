using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using WebApplication1.Data.Models;

namespace WebApplication1.Data
{
    public class AbstaractShardingDbContext : IdentityDbContext, IShardingDbContext, ISupportShardingReadWrite, IShardingTableDbContext
    {


        public AbstaractShardingDbContext([NotNull] DbContextOptions<AbstaractShardingDbContext> options) : base(options)
        {
            var wrapOptionsExtension = options.FindExtension<ShardingWrapOptionsExtension>();
            if (wrapOptionsExtension != null)
                _shardingDbContextExecutor = new ShardingDbContextExecutor(this);
        }

        #region 接口实现

        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;

        /// <summary>
        /// 是否是真正的执行者
        /// </summary>
        private bool IsExecutor => _shardingDbContextExecutor == null;

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
        /// 提交
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Commit()
        {
            _shardingDbContextExecutor.Commit();
        }

        /// <summary>
        /// 异步提交
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _shardingDbContextExecutor.CommitAsync(cancellationToken);
        }

        // 检查和设置分片键支持自动创建
        //private void CheckAndSetShardingKeyThatSupportAutoCreate<TEntity>(TEntity entity) where TEntity : class {
        //  if (entity is IShardingKeyIsGuId) {

        //    if (entity is IEntity<Guid> guidEntity) {
        //      if (guidEntity.Id != default) {
        //        return;
        //      }
        //      var idProperty = entity.GetObjectProperty(nameof(IEntity<Guid>.Id));

        //      var dbGeneratedAttr = ReflectionHelper
        //          .GetSingleAttributeOrDefault<DatabaseGeneratedAttribute>(
        //              idProperty
        //          );

        //      if (dbGeneratedAttr != null && dbGeneratedAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None) {
        //        return;
        //      }

        //      EntityHelper.TrySetId(
        //          guidEntity,
        //          () => GuidGenerator.Create(),
        //          true
        //      );
        //    }
        //  } else if (entity is IShardingKeyIsCreationTime) {
        //    AuditPropertySetter?.SetCreationProperties(entity);
        //  }
        //}

        /// <summary>
        /// 根据对象创建通用的dbcontext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public DbContext CreateGenericDbContext<T>(T entity) where T : class
        {
            //CheckAndSetShardingKeyThatSupportAutoCreate(entity);
            var dbContext = _shardingDbContextExecutor.CreateGenericDbContext(entity);
            //if (dbContext is AbpDbContext<TDbContext> abpDbContext && abpDbContext.LazyServiceProvider == null) {
            //  abpDbContext.LazyServiceProvider = this.LazyServiceProvider;
            //}

            return dbContext;
        }

        /// <summary>
        /// 获取数据上下文
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="parallelQuery"></param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public DbContext GetDbContext(string dataSourceName, CreateDbContextStrategyEnum parallelQuery, IRouteTail routeTail)
        {
            var dbContext = _shardingDbContextExecutor.CreateDbContext(parallelQuery, dataSourceName, routeTail);
            //if (!parallelQuery && dbContext is AbpDbContext<TDbContext> abpDbContext) {
            //  abpDbContext.LazyServiceProvider = this.LazyServiceProvider;
            //}

            return dbContext;
        }

        /// <summary>
        /// 获取虚拟数据源
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IVirtualDataSource GetVirtualDataSource()
        {
            return _shardingDbContextExecutor.GetVirtualDataSource();
        }

        /// <summary>
        /// 通知分库分表事务
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void NotifyShardingTransaction()
        {
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Rollback()
        {
            _shardingDbContextExecutor.Rollback();
        }

        /// <summary>
        /// 异步回滚
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            return _shardingDbContextExecutor.RollbackAsync(cancellationToken);
        }

        #endregion

        #region 重写dbcontext的方法

        public override EntityEntry Add(object entity)
        {
            if (IsExecutor) base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            if (IsExecutor) return base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor) return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsExecutor) return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }


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

        public override int SaveChanges()
        {

            if (IsExecutor) return base.SaveChanges();
            return this.SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (IsExecutor)
                return base.SaveChanges(acceptAllChangesOnSuccess);
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (Database.CurrentTransaction == null && _shardingDbContextExecutor.IsMultiDbContext)
            {
                using (var tran = Database.BeginTransaction())
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
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            //ApplyShardingConcepts();
            int i = 0;
            //如果是内部开的事务就内部自己消化
            if (Database.CurrentTransaction == null && _shardingDbContextExecutor.IsMultiDbContext)
            {
                using (var tran = await Database.BeginTransactionAsync(cancellationToken))
                {
                    i = await _shardingDbContextExecutor.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

                    await tran.CommitAsync(cancellationToken);
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

        #endregion

        public IRouteTail RouteTail { get; set; }

        public DbSet<TestModel> TestModels { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<GuidShardingTable> GuidShardingTables { get; set; }

        public DbSet<TestModelKey> TestModelKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TestModelKey>().HasKey(t => new { t.Id, t.Key });
            builder.Entity<TestModelKey>().Property(o => o.Key).IsRequired().HasMaxLength(36);
        }
    }
}
