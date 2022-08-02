using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.EFCores;
using ShardingCore.EFCores.OptionsExtensions;
using ShardingCore.Exceptions;
using ShardingCore.Utils;

namespace ShardingCore.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DbContextExtension
    {
        /// <summary>
        /// 移除所有的分表关系的模型
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextRelationModelThatIsShardingTable(this DbContext dbContext)
        {
#if !EFCORE2&&!EFCORE3&&!EFCORE5&&!EFCORE6
            throw new NotImplementedException();
#endif
#if EFCORE6

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;
#endif
#if EFCORE2 || EFCORE3 || EFCORE5

            var contextModel = dbContext.Model as Model;
#endif
            var shardingRuntimeContext = dbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();

#if EFCORE6
            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
            var valueTuples =
                contextModelRelationalModel.Tables.Where(o =>o.Value.EntityTypeMappings.Any(m => entityMetadataManager.IsShardingTable(m.EntityType.ClrType))).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples =
 contextModelRelationalModel.Tables.Where(o => o.Value.EntityTypeMappings.Any(m => entityMetadataManager.IsShardingTable(m.EntityType.ClrType))).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if EFCORE2 || EFCORE3
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            var list = entityTypes.Where(o=>entityMetadataManager.IsShardingTable(o.Value.ClrType)).Select(o=>o.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                entityTypes.Remove(list[i]);
            }
#endif
        }

        //        /// <summary>
        //        /// 移除所有的没有分片的表
        //        /// </summary>
        //        /// <param name="dbContext"></param>
        //        public static void RemoveDbContextAllRelationModel(this DbContext dbContext)
        //        {
        //#if EFCORE6

        //            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;
        //#endif
        //#if EFCORE2 || EFCORE3 || EFCORE5

        //            var contextModel = dbContext.Model as Model;
        //#endif
        //#if EFCORE6
        //            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
        //            contextModelRelationalModel.Tables.Clear();
        //#endif
        //#if EFCORE5
        //            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
        //            contextModelRelationalModel.Tables.Clear();
        //#endif
        //#if EFCORE2 || EFCORE3
        //            var entityTypes =
        //                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
        //            entityTypes.Clear();
        //#endif
        //        }
        /// <summary>
        /// 移除所有除了仅分库的
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextAllRelationModelWithoutShardingDataSourceOnly(this DbContext dbContext)
        {
#if !EFCORE2&&!EFCORE3&&!EFCORE5&&!EFCORE6
            throw new NotImplementedException();
#endif
#if EFCORE6

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;
#endif
#if EFCORE2 || EFCORE3 || EFCORE5

            var contextModel = dbContext.Model as Model;
#endif
            var shardingRuntimeContext = dbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();

#if EFCORE6
            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
            var valueTuples =
                contextModelRelationalModel.Tables.Where(o => o.Value.EntityTypeMappings.Any(m => !entityMetadataManager.IsShardingDataSource(m.EntityType.ClrType) ||entityMetadataManager.TryGet(m.EntityType.ClrType)==null)).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            var valueTuples =
 contextModelRelationalModel.Tables.Where(o => o.Value.EntityTypeMappings.Any(m => !entityMetadataManager.IsShardingDataSource(m.EntityType.ClrType)||entityMetadataManager.TryGet(m.EntityType.ClrType)==null)).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if EFCORE2 || EFCORE3
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            var list = entityTypes.Where(o => !entityMetadataManager.IsShardingDataSource(o.Value.ClrType) || entityMetadataManager.TryGet(o.Value.ClrType) == null).Select(o => o.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                entityTypes.Remove(list[i]);
            }
#endif
        }
        /// <summary>
        /// 移除所有的表
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextAllRelationModel(this DbContext dbContext)
        {
#if !EFCORE2&&!EFCORE3&&!EFCORE5&&!EFCORE6
            throw new NotImplementedException();
#endif
#if EFCORE6

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;
#endif
#if EFCORE2 || EFCORE3 || EFCORE5

            var contextModel = dbContext.Model as Model;
#endif

#if EFCORE6
            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
            contextModelRelationalModel.Tables.Clear();
#endif
#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            contextModelRelationalModel.Tables.Clear();
#endif
#if EFCORE2 || EFCORE3
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            entityTypes.Clear();
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
#if !EFCORE2 && !EFCORE3 && !EFCORE5 && !EFCORE6
            1
#endif
#if EFCORE2 ||EFCORE3 ||EFCORE5

            var contextModel = dbContext.Model as Model;
#endif
#if EFCORE6
            var contextModel = dbContext.GetService<IDesignTimeModel>().Model;
            var entityTypes = contextModel.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                if (entityType.GetFieldValue("_data") is List<object> _data)
                {
                    _data.Clear();
                }
            }
            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
            var valueTuples =
                contextModelRelationalModel.Tables
                    .Where(o => o.Value.EntityTypeMappings.All(m => m.EntityType.ClrType != shardingType))
                    .Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif

#if EFCORE5
            var contextModelRelationalModel = contextModel.RelationalModel as RelationalModel;
            
            var entityTypes = contextModel.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                if (entityType.GetFieldValue("_data") is List<object> _data)
                {
                    _data.Clear();
                }
            }
            var valueTuples = contextModelRelationalModel.Tables
                .Where(o => o.Value.EntityTypeMappings.All(m => m.EntityType.ClrType != shardingType))
                .Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
#endif
#if EFCORE2 || EFCORE3
            var entityTypes =
                contextModel.GetFieldValue("_entityTypes") as SortedDictionary<string, EntityType>;
            
            foreach (var entityType in entityTypes)
            {
                if (entityType.Value.GetFieldValue("_data") is List<object> _data)
                {
                    _data.Clear();
                }
            }
            var list = entityTypes.Where(o=>o.Value.ClrType!=shardingType).Select(o=>o.Key).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                entityTypes.Remove(list[i]);
            }
#endif
        }

        public static void RemoveDbContextRelationModelSaveOnlyThatIsNamedType<T>(this DbContext dbContext)
            where T:class
        {
            RemoveDbContextRelationModelSaveOnlyThatIsNamedType(dbContext, typeof(T));
        }

        /// <summary>
        /// 移除模型缓存
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveModelCache(this DbContext dbContext)
        {
#if !EFCORE2&&!EFCORE3&&!EFCORE5&&!EFCORE6
            throw new NotImplementedException();
#endif
#if EFCORE6
            var shardingModelSource = dbContext.GetService<IModelSource>() as IShardingModelSource;
            var modelCacheKeyFactory = shardingModelSource.GetModelCacheKeyFactory();
            object key1 = modelCacheKeyFactory.Create(dbContext,true);
            shardingModelSource.Remove(key1);
            object key2 = modelCacheKeyFactory.Create(dbContext,false);
            shardingModelSource.Remove(key2);
#endif
#if EFCORE5
            var shardingModelSource = dbContext.GetService<IModelSource>() as IShardingModelSource;
            var modelCacheKeyFactory = shardingModelSource.GetModelCacheKeyFactory();
            object key1 = modelCacheKeyFactory.Create(dbContext);
            shardingModelSource.Remove(key1);
#endif
#if EFCORE3
           
            var shardingModelSource = dbContext.GetService<IModelSource>()  as IShardingModelSource;
            var modelCacheKeyFactory = shardingModelSource.GetModelCacheKeyFactory();
            object key1 = modelCacheKeyFactory.Create(dbContext);
            shardingModelSource.Remove(key1);
#endif

#if EFCORE2

            var shardingModelSource = dbContext.GetService<IModelSource>() as IShardingModelSource;
            var modelCacheKeyFactory = shardingModelSource.GetModelCacheKeyFactory();
            object key1 = modelCacheKeyFactory.Create(dbContext);
            shardingModelSource.Remove(key1);
#endif
        }

        /// <summary>
        /// 获取模型创建的锁
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static object GetModelCacheSyncObject(this DbContext dbContext)
        {
            IShardingModelSource shardingModelSource = dbContext.GetService<IModelSource>() as IShardingModelSource;
            return shardingModelSource.GetSyncObject();
        }


        public static IEnumerable<object> GetPrimaryKeyValues<TEntity>(TEntity entity,IKey primaryKey) where TEntity : class
        {
            return primaryKey.Properties.Select(o =>entity.GetPropertyValue(o.Name));
        }

        public static TEntity GetAttachedEntity<TEntity>(this DbContext context, TEntity entity) where TEntity:class
        {
            if (entity == null) { throw new ArgumentNullException(nameof(entity)); }
            var entityPrimaryKey = context.Model.FindEntityType(entity.GetType()).FindPrimaryKey();
            if (entityPrimaryKey == null)
            {
                return entity;
            }
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

        public static IShardingRuntimeContext GetShardingRuntimeContext(this DbContext dbContext)
        {
            var shardingRuntimeContext =  dbContext.GetService<IShardingRuntimeContext>();
           
            if (shardingRuntimeContext == null)
            {
                throw new ShardingCoreInvalidOperationException($"cant resolve:[{typeof(IShardingRuntimeContext)}],dbcontext:[{dbContext}]");
            }

            return shardingRuntimeContext;
        }

    }
}