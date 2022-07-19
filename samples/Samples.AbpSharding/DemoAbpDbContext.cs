// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace Samples.AbpSharding
{
    public abstract class AbstractShardingAbpZeroDbContext<TTenant, TRole, TUser, TSelf>
        : AbpZeroDbContext<TTenant, TRole, TUser, TSelf>,
        IShardingDbContext,
        ISupportShardingReadWrite,
        IShardingTableDbContext
        where TTenant : AbpTenant<TUser>
        where TRole : AbpRole<TUser>
        where TUser : AbpUser<TUser>
        where TSelf : AbpZeroDbContext<TTenant, TRole, TUser, TSelf>
    {
        private readonly IShardingDbContextExecutor _shardingDbContextExecutor;

        protected AbstractShardingAbpZeroDbContext(DbContextOptions<TSelf> options)
            : base(options)
        {
            var wrapOptionsExtension = options.FindExtension<ShardingWrapOptionsExtension>();
            if (wrapOptionsExtension != null)
            {
                _shardingDbContextExecutor = new ShardingDbContextExecutor(this);
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
        protected bool isExecutor => _shardingDbContextExecutor == null;

        /// <summary>
        /// 路由表
        /// </summary>
        public IRouteTail RouteTail { get; set; }


        public override EntityEntry Add(object entity)
        {
            if (isExecutor)
                base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            if (isExecutor)
                return base.Add(entity);
            return CreateGenericDbContext(entity).Add(entity);
        }


        public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (isExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
        {
            if (isExecutor)
                return base.AddAsync(entity, cancellationToken);
            return CreateGenericDbContext(entity).AddAsync(entity, cancellationToken);
        }

        public override void AddRange(params object[] entities)
        {
            if (isExecutor)
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
            if (isExecutor)
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
            if (isExecutor)
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
            if (isExecutor)
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
            if (isExecutor)
                return base.Attach(entity);
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override EntityEntry Attach(object entity)
        {
            if (isExecutor)
                return base.Attach(entity);
            return CreateGenericDbContext(entity).Attach(entity);
        }

        public override void AttachRange(params object[] entities)
        {
            if (isExecutor)
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
            if (isExecutor)
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

        public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        {
            if (isExecutor)
                return base.Entry(entity);
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry Entry(object entity)
        {
            if (isExecutor)
                return base.Entry(entity);
            return CreateGenericDbContext(entity).Entry(entity);
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            if (isExecutor)
                return base.Update(entity);
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            if (isExecutor)
                return base.Update(entity);
            return CreateGenericDbContext(entity).Update(entity);
        }

        public override void UpdateRange(params object[] entities)
        {
            if (isExecutor)
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
            if (isExecutor)
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
            if (isExecutor)
                return base.Remove(entity);
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override EntityEntry Remove(object entity)
        {
            if (isExecutor)
                return base.Remove(entity);
            return CreateGenericDbContext(entity).Remove(entity);
        }

        public override void RemoveRange(params object[] entities)
        {
            if (isExecutor)
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
            if (isExecutor)
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

            if (isExecutor)
                return base.SaveChanges();
            return this.SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (isExecutor)
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
            if (isExecutor)
                return base.SaveChangesAsync(cancellationToken);
            return this.SaveChangesAsync(true, cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if (isExecutor)
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

            if (isExecutor)
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
            if (isExecutor)
            {
                await base.DisposeAsync();
            }
            else
            {
                await _shardingDbContextExecutor.DisposeAsync();

                await base.DisposeAsync();
            }
        }


        #region Sharding Core 方法实现

        /// <summary>
        /// 根据查询获取DbContext
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="strategy"></param>
        /// <param name="routeTail"></param>
        /// <returns></returns>
        public DbContext GetDbContext(string dataSourceName, CreateDbContextStrategyEnum strategy, IRouteTail routeTail)
        {
            var dbContext = _shardingDbContextExecutor.CreateDbContext(
                strategy,
                dataSourceName,
                routeTail
                );

            this.FillDbContextInject(dbContext);

            return dbContext;
        }

        /// <summary>
        /// 根据对象创建通用的dbcontext
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbContext CreateGenericDbContext<TEntity>(TEntity entity) where TEntity : class
        {
            CheckAndSetShardingKeyThatSupportAutoCreate(entity);

            var dbContext = _shardingDbContextExecutor.CreateGenericDbContext(entity);

            this.FillDbContextInject(dbContext);

            return dbContext;
        }

        public virtual IVirtualDataSource GetVirtualDataSource()
        {
            return _shardingDbContextExecutor.GetVirtualDataSource();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _shardingDbContextExecutor.RollbackAsync(cancellationToken);
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _shardingDbContextExecutor.CommitAsync(cancellationToken);
        }

        public virtual void NotifyShardingTransaction()
        {
            _shardingDbContextExecutor.NotifyShardingTransaction();
        }

        public virtual void Rollback()
        {
            _shardingDbContextExecutor.Rollback();
        }

        public virtual void Commit()
        {
            _shardingDbContextExecutor.Commit();
        }

        #endregion


        #region 内部方法

        /// <summary>
        /// 检查并设置分表键值是否支持自动创建
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        protected virtual void CheckAndSetShardingKeyThatSupportAutoCreate<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is IShardingKeyIsGuId)
            {

                if (entity is IEntity<Guid> guidEntity)
                {
                    if (guidEntity.Id != default)
                    {
                        return;
                    }
                    var idProperty = entity.GetObjectProperty(nameof(IEntity<Guid>.Id));

                    var dbGeneratedAttr = idProperty.GetCustomAttributes(false)
                        .Where(o => o.GetType() == typeof(DatabaseGeneratedAttribute))
                        .FirstOrDefault() as DatabaseGeneratedAttribute;

                    //var dbGeneratedAttr = ReflectionHelper
                    //    .GetSingleAttributeOrDefault<DatabaseGeneratedAttribute>(
                    //        idProperty
                    //    );

                    if (dbGeneratedAttr != null && dbGeneratedAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                    {
                        return;
                    }

                    guidEntity.Id = GuidGenerator.Create();
                }
            }
            else if (entity is IShardingKeyIsCreationTime)
            {
                this.SetCreationAuditProperties(entity, this.GetAuditUserId());

                //AuditPropertySetter?.SetCreationProperties(entity);
            }
        }

        /// <summary>
        /// 填充DbContext需要的依赖项
        /// </summary>
        /// <param name="dbContext"></param>
        protected virtual void FillDbContextInject(DbContext dbContext)
        {
            if (dbContext is AbpZeroCommonDbContext<TRole, TUser, TSelf> abpDbContext)
            {
                // AbpZeroCommonDbContext
                if (abpDbContext.EntityHistoryHelper == null)
                {
                    abpDbContext.EntityHistoryHelper = this.EntityHistoryHelper;
                }

                // AbpDbContext
                if (abpDbContext.AbpSession == null)
                {
                    abpDbContext.AbpSession = this.AbpSession;
                }
                if (abpDbContext.EntityChangeEventHelper == null)
                {
                    abpDbContext.EntityChangeEventHelper = this.EntityChangeEventHelper;
                }
                if (abpDbContext.Logger == null)
                {
                    abpDbContext.Logger = this.Logger;
                }
                if (abpDbContext.EventBus == null)
                {
                    abpDbContext.EventBus = this.EventBus;
                }
                if (abpDbContext.GuidGenerator == null)
                {
                    abpDbContext.GuidGenerator = this.GuidGenerator;
                }
                if (abpDbContext.CurrentUnitOfWorkProvider == null)
                {
                    abpDbContext.CurrentUnitOfWorkProvider = this.CurrentUnitOfWorkProvider;
                }
                if (abpDbContext.MultiTenancyConfig == null)
                {
                    abpDbContext.MultiTenancyConfig = this.MultiTenancyConfig;
                }
                abpDbContext.SuppressAutoSetTenantId = this.SuppressAutoSetTenantId;
            }
        }

        #endregion

    }
}
