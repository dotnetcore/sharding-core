
#if SHARDINGCORE7
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Threading;

namespace ShardingCore.EFCores
{

    public class ShardingModelSource : ModelSource, IShardingModelSource
    {
        private readonly object _syncObject = new();

        public ShardingModelSource(ModelSourceDependencies dependencies) : base(dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected override ModelSourceDependencies Dependencies { get; }


        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context">The context the model is being produced for.</param>
        /// <param name="modelCreationDependencies">The dependencies object used during the creation of the model.</param>
        /// <param name="designTime">Whether the model should contain design-time configuration.</param>
        /// <returns>The model to be used.</returns>
        public override IModel GetModel(
            DbContext context,
            ModelCreationDependencies modelCreationDependencies,
            bool designTime)
        {
            var priority = CacheItemPriority.High;
            var size = 200;
            var waitSeconds = 3;
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is null)
                {
                    throw new ShardingCoreInvalidOperationException("db context model is inited before RouteTail set value");
                }
                if (shardingTableDbContext.RouteTail is INoCacheRouteTail)
                {
                    var noCacheModel = this.CreateModel(context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);
                    noCacheModel = modelCreationDependencies.ModelRuntimeInitializer.Initialize(noCacheModel, designTime, modelCreationDependencies.ValidationLogger);
                    return noCacheModel;
                }
                else 
                if (shardingTableDbContext.RouteTail is ISingleQueryRouteTail singleQueryRouteTail && singleQueryRouteTail.IsShardingTableQuery())
                {
                    priority = CacheItemPriority.Normal;
                }
            }
            var cache = Dependencies.MemoryCache;
            var cacheKey = Dependencies.ModelCacheKeyFactory.Create(context, designTime);
            if (!cache.TryGetValue(cacheKey, out IModel model))
            {
                if (context is IShardingModelCacheOption shardingModelCacheOption)
                {
                    priority = shardingModelCacheOption.GetModelCachePriority();
                    size = shardingModelCacheOption.GetModelCacheEntrySize();
                    waitSeconds = shardingModelCacheOption.GetModelCacheLockObjectSeconds();
                }
                // Make sure OnModelCreating really only gets called once, since it may not be thread safe.
                var acquire = Monitor.TryEnter(_syncObject, TimeSpan.FromSeconds(waitSeconds));
                if (!acquire)
                {
                    throw new ShardingCoreInvalidOperationException("cache model timeout");
                }
                try
                {
                    if (!cache.TryGetValue(cacheKey, out model))
                    {
                        model = CreateModel(
                            context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);

                        model = modelCreationDependencies.ModelRuntimeInitializer.Initialize(
                            model, designTime, modelCreationDependencies.ValidationLogger);

                        model = cache.Set(cacheKey, model, new MemoryCacheEntryOptions { Size = size, Priority = priority });
                    }
                }
                finally
                {
                    Monitor.Exit(_syncObject);
                }
            }

            return model;
        }

        public IModelCacheKeyFactory GetModelCacheKeyFactory()
        {
            return Dependencies.ModelCacheKeyFactory;
        }

        public object GetSyncObject()
        {
            return _syncObject;
        }

        public void Remove(object key)
        {
            var acquire = Monitor.TryEnter(_syncObject, TimeSpan.FromSeconds(3));
            if (!acquire)
            {
                throw new ShardingCoreInvalidOperationException("cache model timeout");
            }
            try
            {

                var cache = Dependencies.MemoryCache;
                cache.Remove(key);
            }
            finally
            {
                Monitor.Exit(_syncObject);
            }
        }
    }
}
#endif