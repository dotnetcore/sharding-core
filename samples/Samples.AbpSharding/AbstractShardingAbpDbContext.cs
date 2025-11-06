using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.EFCores;
using ShardingCore.Sharding.ShardingDbContextExecutors;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Reflection;

namespace Samples.AbpSharding
{

    public abstract class AbstractShardingAbpDbContext<TDbContext> : AbpDbContext<TDbContext>, IShardingDbContext
                                where TDbContext : DbContext
    {
        private bool _createExecutor = false;
        protected AbstractShardingAbpDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
        }


        private IShardingDbContextExecutor _shardingDbContextExecutor;
        public IShardingDbContextExecutor GetShardingExecutor()
        {
            if (!_createExecutor)
            {
                _shardingDbContextExecutor=this.DoCreateShardingDbContextExecutor();
                _createExecutor = true;
            }
            return _shardingDbContextExecutor;
        }

        private IShardingDbContextExecutor DoCreateShardingDbContextExecutor()
        {
            var shardingDbContextExecutor = this.CreateShardingDbContextExecutor();
            if (shardingDbContextExecutor != null)
            {
                
                shardingDbContextExecutor.EntityCreateDbContextBefore += (sender, args) =>
                {
                    CheckAndSetShardingKeyThatSupportAutoCreate(args.Entity);
                };
                shardingDbContextExecutor.CreateDbContextAfter += (sender, args) =>
                {
                    var dbContext = args.DbContext;
                    if (dbContext is AbpDbContext<TDbContext> abpDbContext && abpDbContext.LazyServiceProvider == null)
                    {
                        abpDbContext.LazyServiceProvider = this.LazyServiceProvider;
                        if (dbContext is IAbpEfCoreDbContext abpEfCoreDbContext&&this.UnitOfWorkManager.Current!=null)
                        {
                            abpEfCoreDbContext.Initialize(
                                new AbpEfCoreDbContextInitializationContext(
                                    this.UnitOfWorkManager.Current
                                )
                            );
                        }
                    }
                };
            }
            return shardingDbContextExecutor;
        }


        private void CheckAndSetShardingKeyThatSupportAutoCreate<TEntity>(TEntity entity) where TEntity : class
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

                    var dbGeneratedAttr = ReflectionHelper
                        .GetSingleAttributeOrDefault<DatabaseGeneratedAttribute>(
                            idProperty
                        );

                    if (dbGeneratedAttr != null && dbGeneratedAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                    {
                        return;
                    }

                    EntityHelper.TrySetId(
                        guidEntity,
                        () => GuidGenerator.Create(),
                        true
                    );
                }
            }
            else if (entity is IShardingKeyIsCreationTime)
            {
                AuditPropertySetter?.SetCreationProperties(entity);
            }
        }


        /// <summary>
        /// abp 5.x+ 如果存在并发字段那么需要添加这段代码
        /// </summary>
        protected override void HandlePropertiesBeforeSave()
        {
            if (GetShardingExecutor() == null)
            {
                base.HandlePropertiesBeforeSave();
            }
        }


        // /// <summary>
        // /// abp 4.x+ 如果存在并发字段那么需要添加这段代码
        // /// </summary>
        // /// <returns></returns>
        //
        // protected override void ApplyAbpConcepts(EntityEntry entry, EntityChangeReport changeReport)
        // {
        //     if (GetShardingExecutor() == null)
        //     {
        //         base.ApplyAbpConcepts(entry, changeReport);
        //     }
        // }
        
        /// <summary>
        /// 创建领域事件报告
        /// </summary>
        /// <returns></returns>
        protected override EntityEventReport CreateEventReport()
        {
            if (GetShardingExecutor() == null)
            {
                return base.CreateEventReport();
            }

            return new EntityEventReport();
        }

        /// <summary>
        /// 发布实体事件
        /// </summary>
        /// <param name="changeReport"></param>
        protected override void PublishEntityEvents(EntityEventReport changeReport)
        {
            if (GetShardingExecutor() == null)
            {
                base.PublishEntityEvents(changeReport);
            }
        }

        /// <summary>
        /// 发布变更实体的事件
        /// </summary>
        /// <returns></returns>
        protected override Task PublishEventsForChangedEntityOnSaveChangeAsync()
        {
            if (GetShardingExecutor() == null)
            {
                return base.PublishEventsForChangedEntityOnSaveChangeAsync();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 重写异步保存方法去重审计日志中的实体变更项
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            var list = AuditingManager?.Current?.Log.EntityChanges.DistinctBy(p => new
            {
                p.EntityTypeFullName,
                p.EntityId,
                p.ChangeType
            }).ToList();
            // 判断是否存在重复的变更项
            if (list == null || list.Count == 0 || list.Count == AuditingManager.Current.Log.EntityChanges.Count)
            {
                return result;
            }

            AuditingManager.Current.Log.EntityChanges.Clear();
            foreach (var item in list)
            {
                AuditingManager.Current.Log.EntityChanges.Add(item);
            }

            return result;
        }

        public override void Dispose()
        {
            _shardingDbContextExecutor?.Dispose();
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            if (_shardingDbContextExecutor != null)
            {
                await _shardingDbContextExecutor.DisposeAsync();
            }
            await base.DisposeAsync();
        }
    }
}