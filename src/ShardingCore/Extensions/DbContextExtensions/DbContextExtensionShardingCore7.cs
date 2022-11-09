#if SHARDINGCORE7
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.EFCores;
using ShardingCore.Exceptions;

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

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;
            var shardingRuntimeContext = dbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();

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
                contextModelRelationalModel.Tables.Where(o =>o.Value.EntityTypeMappings.Any(m => entityMetadataManager.IsShardingTable(m.EntityType.ClrType))).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }
        }
        /// <summary>
        /// 移除所有除了仅分库的
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextAllRelationModelWithoutShardingDataSourceOnly(this DbContext dbContext)
        {

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;

            var shardingRuntimeContext = dbContext.GetShardingRuntimeContext();
            var entityMetadataManager = shardingRuntimeContext.GetEntityMetadataManager();

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
                contextModelRelationalModel.Tables.Where(o => o.Value.EntityTypeMappings.Any(m => !entityMetadataManager.IsShardingDataSource(m.EntityType.ClrType) ||entityMetadataManager.TryGet(m.EntityType.ClrType)==null)).Select(o => o.Key).ToList();
            for (int i = 0; i < valueTuples.Count; i++)
            {
                contextModelRelationalModel.Tables.Remove(valueTuples[i]);
            }

        }
        /// <summary>
        /// 移除所有的表
        /// </summary>
        /// <param name="dbContext"></param>
        public static void RemoveDbContextAllRelationModel(this DbContext dbContext)
        {

            var contextModel = dbContext.GetService<IDesignTimeModel>().Model; ;

            var contextModelRelationalModel = contextModel.GetRelationalModel() as RelationalModel;
            contextModelRelationalModel.Tables.Clear();
        }

        /// <summary>
        /// 移除所有的除了我指定的那个类型
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="shardingType"></param>
        public static void RemoveDbContextRelationModelSaveOnlyThatIsNamedType(this DbContext dbContext,
            Type shardingType)
        {
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
            var shardingModelSource = dbContext.GetService<IModelSource>() as IShardingModelSource;
            var modelCacheKeyFactory = shardingModelSource.GetModelCacheKeyFactory();
            object key1 = modelCacheKeyFactory.Create(dbContext,true);
            shardingModelSource.Remove(key1);
            object key2 = modelCacheKeyFactory.Create(dbContext,false);
            shardingModelSource.Remove(key2);

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
            var entityPrimaryKey = context.Model.FindRuntimeEntityType(entity.GetType()).FindPrimaryKey();
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

#endif