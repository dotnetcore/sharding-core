#if EFCORE3
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingModelSource: ModelSource, IShardingModelSource
    {

        private readonly object _syncObject = new object();

        /// <summary>
        ///     Creates a new <see cref="ModelSource" /> instance.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public ShardingModelSource( ModelSourceDependencies dependencies):base(dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelSource" />
        /// </summary>
        protected override ModelSourceDependencies Dependencies { get; }

        /// <summary>
        ///     Returns the model from the cache, or creates a model if it is not present in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <returns> The model to be used. </returns>
        public override IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder)
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
                    var noCacheModel = CreateModel(context, conventionSetBuilder);
                    return noCacheModel;
                }
                else if (shardingTableDbContext.RouteTail is ISingleQueryRouteTail singleQueryRouteTail && singleQueryRouteTail.IsShardingTableQuery())
                {
                    priority = CacheItemPriority.Normal;
                }
            }
            if (context is IShardingModelCacheOption shardingModelCacheOption)
            {
                priority = shardingModelCacheOption.GetModelCachePriority();
                size = shardingModelCacheOption.GetModelCacheEntrySize();
                waitSeconds = shardingModelCacheOption.GetModelCacheLockObjectSeconds();
            }
            var cache = Dependencies.MemoryCache;
            var cacheKey = Dependencies.ModelCacheKeyFactory.Create(context);
            if (!cache.TryGetValue(cacheKey, out IModel model))
            {
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
                        model = CreateModel(context, conventionSetBuilder);
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