﻿#if EFCORE5
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.EFCores
{
    public class ShardingModelSource : ModelSource
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        /// <summary>
        ///     Creates a new <see cref="ModelSource" /> instance.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public ShardingModelSource([NotNull] ModelSourceDependencies dependencies,IShardingRuntimeContext shardingRuntimeContext) : base(dependencies)
        {
            _shardingRuntimeContext = shardingRuntimeContext;

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
        [Obsolete("Use the overload with ModelDependencies")]
        public override IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder)
        {
            throw new ShardingCoreNotSupportException("Use the overload with ModelDependencies");
        }

        /// <summary>
        ///     Returns the model from the cache, or creates a model if it is not present in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="modelDependencies"> The dependencies object for the model. </param>
        /// <returns> The model to be used. </returns>
        public override IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder,
            ModelDependencies modelDependencies)
        {

            CacheItemPriority? setPriority = null;
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is null)
                {
                    throw new ShardingCoreInvalidOperationException("db context model is inited before RouteTail set value");
                }
                if (shardingTableDbContext.RouteTail is INoCacheRouteTail)
                {
                    var noCacheModel = CreateModel(context, conventionSetBuilder, modelDependencies);
                    return noCacheModel;
                }
                else if (shardingTableDbContext.RouteTail is ISingleQueryRouteTail singleQueryRouteTail&& singleQueryRouteTail.IsShardingTableQuery())
                {
                    setPriority = CacheItemPriority.Normal;
                }
            }
            var cache = Dependencies.MemoryCache;
            var cacheKey = Dependencies.ModelCacheKeyFactory.Create(context);
            if (!cache.TryGetValue(cacheKey, out IModel model))
            {
                var modelCacheLockerProvider = _shardingRuntimeContext.GetModelCacheLockerProvider();

                var priority = setPriority ?? modelCacheLockerProvider.GetCacheItemPriority();
                var size = modelCacheLockerProvider.GetCacheEntrySize();
                var waitSeconds = modelCacheLockerProvider.GetCacheModelLockObjectSeconds();
                var cacheLockObject = modelCacheLockerProvider.GetCacheLockObject(cacheKey);
                // Make sure OnModelCreating really only gets called once, since it may not be thread safe.
                var acquire = Monitor.TryEnter(cacheLockObject, TimeSpan.FromSeconds(waitSeconds));
                if (!acquire)
                {
                    //如果排查后非循环注入等操作导致的确实是并发一瞬间导致的timeout比如上千上万个分片后缀那么可以将模型level设置高并且cacheEntrySize设置为1超时时间设置大一点即可
                    //如果是abp那么请确认是否使用了AsyncExecutor
                    //https://github.com/dotnetcore/sharding-core/issues/221
                    throw new ShardingCoreInvalidOperationException("cache model timeout");
                }
                try
                {
                    if (!cache.TryGetValue(cacheKey, out model))
                    {
                        model = CreateModel(context, conventionSetBuilder, modelDependencies);
                        model = cache.Set(cacheKey, model, new MemoryCacheEntryOptions { Size = size, Priority = priority });
                    }
                }
                finally
                {
                    Monitor.Exit(cacheLockObject);
                }
            }

            return model;
        }
    }
}

#endif