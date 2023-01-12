using Microsoft.EntityFrameworkCore;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Reflection;

namespace Samples.AbpSharding
{

    public abstract class AbstractShardingAbpDbContext<TDbContext> : AbpDbContext<TDbContext>, IShardingDbContext
                                where TDbContext : DbContext
    {
        protected AbstractShardingAbpDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
        }


        private IShardingDbContextExecutor _shardingDbContextExecutor;
        public IShardingDbContextExecutor GetShardingExecutor()
        {
            return _shardingDbContextExecutor??=DoCreateShardingDbContextExecutor();
        }

        private IShardingDbContextExecutor DoCreateShardingDbContextExecutor()
        {
            var shardingDbContextExecutor = this.CreateShardingDbContextExecutor()!;
            
            shardingDbContextExecutor.EntityCreateDbContextBefore += (sender, args) =>
            {
                CheckAndSetShardingKeyThatSupportAutoCreate(args.Entity);
            };
            shardingDbContextExecutor.CreateDbContextAfter += (sender, args) =>
            {
                var argsDbContext = args.DbContext;
              
                if (argsDbContext is AbpDbContext<TDbContext> abpDbContext&&
                    abpDbContext.LazyServiceProvider == null)
                {
                    abpDbContext.LazyServiceProvider = this.LazyServiceProvider;
                }
            };
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