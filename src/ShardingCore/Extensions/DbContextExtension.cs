using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;

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

#if EFCORE5
var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples =
 contextModelRelationalModel.Tables.Where(o=>o.Value.EntityTypeMappings.Any(m=>m.EntityType.ClrType.IsShardingTable())).Select(o=>o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if !EFCORE5
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            var list = entityTypes.Where(o=>o.Value.ClrType.IsShardingTable()).Select(o=>o.Key).ToList();
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

    }
}