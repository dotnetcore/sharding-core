using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Utils;

namespace ShardingCore.Extensions
{
    public static class DbContextExtension
    {
        /// <summary>
        /// 移除所有的分表关系的模型
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextRelationModelThatIsShardingTable(this DbContext dbContext)
        {
            var contextModel = dbContext.Model as Model;
            var entityMetadataManager = (IEntityMetadataManager)ShardingContainer.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(dbContext.GetType()));

#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples =
 contextModelRelationalModel.Tables.Where(o => o.Value.EntityTypeMappings.Any(m => entityMetadataManager.IsShardingTable(m.EntityType.ClrType))).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if !EFCORE5
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            var list = entityTypes.Where(o=>entityMetadataManager.IsShardingTable(o.Value.ClrType)).Select(o=>o.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                entityTypes.Remove(list[i]);
            }
#endif
        }

        /// <summary>
        /// 移除所有的除了我指定的那个类型
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="shardingType"></param>
        public static void RemoveDbContextRelationModelSaveOnlyThatIsNamedType(this DbContext dbContext,
            Type shardingType)
        {
            var contextModel = dbContext.Model as Model;

#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples = contextModelRelationalModel.Tables
                .Where(o => o.Value.EntityTypeMappings.All(m => m.EntityType.ClrType != shardingType))
                .Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if !EFCORE5
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            var list = entityTypes.Where(o=>o.Value.ClrType!=shardingType).Select(o=>o.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                entityTypes.Remove(list[i]);
            }
#endif
        }

        public static void RemoveDbContextRelationModelSaveOnlyThatIsNamedType<T>(this DbContext dbContext)
            where T : IShardingTable
        {
            RemoveDbContextRelationModelSaveOnlyThatIsNamedType(dbContext, typeof(T));
        }

        /// <summary>
        /// 移除模型缓存
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveModelCache(this DbContext dbContext)
        {
            var serviceScope = typeof(DbContext).GetTypeFieldValue(dbContext, "_serviceScope") as IServiceScope;
#if EFCORE5
            var dependencies = serviceScope.ServiceProvider.GetService<IModelCreationDependencies>();
            var dependenciesModelSource = dependencies.ModelSource as ModelSource;

            var modelSourceDependencies =
                dependenciesModelSource.GetPropertyValue("Dependencies") as ModelSourceDependencies;
            IMemoryCache memoryCache = modelSourceDependencies.MemoryCache;
            object key1 = modelSourceDependencies.ModelCacheKeyFactory.Create(dbContext);
            memoryCache.Remove(key1);
#endif
#if EFCORE3
           
            var modelSource = serviceScope.ServiceProvider.GetService<IModelSource>();
            var modelSourceImpl = modelSource as ModelSource;
            
            var modelSourceDependencies =
                modelSourceImpl.GetPropertyValue("Dependencies") as ModelSourceDependencies;
            IMemoryCache memoryCache = modelSourceDependencies.MemoryCache;
            object key1 = modelSourceDependencies.ModelCacheKeyFactory.Create(dbContext);
            memoryCache.Remove(key1);
#endif

#if EFCORE2

            var modelSource = serviceScope.ServiceProvider.GetService<IModelSource>();
            var modelSourceImpl = modelSource as RelationalModelSource;

            var modelSourceDependencies =
                modelSourceImpl.GetPropertyValue("Dependencies") as ModelSourceDependencies;
            var models =
                typeof(ModelSource).GetTypeFieldValue(modelSourceImpl, "_models") as ConcurrentDictionary<object, Lazy<IModel>>;
            object key1 = modelSourceDependencies.ModelCacheKeyFactory.Create(dbContext);
            models.TryRemove(key1, out var del);
#endif
        }

        /// <summary>
        /// 获取模型创建的锁
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static object GetModelCacheSyncObject(this DbContext dbContext)
        {
            var serviceScope = typeof(DbContext).GetTypeFieldValue(dbContext, "_serviceScope") as IServiceScope;
#if EFCORE5
            var dependencies = serviceScope.ServiceProvider.GetService<IModelCreationDependencies>();
            var dependenciesModelSource = dependencies.ModelSource as ModelSource;

            var syncObject = dependenciesModelSource.GetFieldValue("_syncObject");
            return syncObject;
#endif
#if EFCORE3
            var modelSource = serviceScope.ServiceProvider.GetService<IModelSource>();
            var modelSourceImpl = modelSource as ModelSource;

            var syncObject = modelSourceImpl.GetFieldValue("_syncObject");
            return syncObject;
#endif
#if EFCORE2
            return sLock;
#endif

        }

        private static object sLock = new object();


        public static IEnumerable<object> GetPrimaryKeyValues<TEntity>(TEntity entity,IKey primaryKey) where TEntity : class
        {
            return primaryKey.Properties.Select(o => entity.GetPropertyValue(o.Name));
        }

        public static TEntity GetAttachedEntity<TEntity>(this DbContext context, TEntity entity) where TEntity:class
        {
            if (entity == null) { throw new ArgumentNullException(nameof(entity)); }
            var entityPrimaryKey = context.Model.FindEntityType(entity.GetType()).FindPrimaryKey();
            var primaryKeyValue = GetPrimaryKeyValues(entity, entityPrimaryKey).ToArray();
            if (primaryKeyValue.IsEmpty())
                return null;
            var dbContextDependencies = (IDbContextDependencies)typeof(DbContext).GetTypePropertyValue(context, "DbContextDependencies");
            var stateManager = dbContextDependencies.StateManager;
            
            //var entityIKey = ShardingKeyUtil.GetEntityIKey(entity);
            var internalEntityEntry = stateManager.TryGetEntry(entityPrimaryKey, primaryKeyValue);
         
            if (internalEntityEntry == null)
                return null;
            return (TEntity)internalEntityEntry.Entity;
            //sp.Restart();

            //var entityEntries = context.ChangeTracker.Entries<TEntity>();
            //sp.Stop();
            //Console.WriteLine($"ChangeTracker.Entries:{sp.ElapsedMilliseconds}毫秒");
            //sp.Restart();
            //var entry = entityEntries.Where(e => e.State != EntityState.Detached && primaryKeyValue.SequenceEqual(ShardingKeyUtil.GetPrimaryKeyValues(e.Entity))).FirstOrDefault();
            //sp.Stop();
            //Console.WriteLine($"ChangeTracker.FirstOrDefault:{sp.ElapsedMilliseconds}毫秒");
            //return entry?.Entity;
        }

    }
}