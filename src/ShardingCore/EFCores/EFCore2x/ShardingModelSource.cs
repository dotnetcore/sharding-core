#if EFCORE2
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ShardingCore.EFCores
{
    public  class ShardingModelSource: ModelSource, IShardingModelSource
    {
        private readonly object _syncObject = new object();
        private readonly ConcurrentDictionary<object, Lazy<IModel>> _models = new ConcurrentDictionary<object, Lazy<IModel>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <returns> The model to be used. </returns>
        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
        {
            if (context is IShardingTableDbContext shardingTableDbContext)
            {
                if (shardingTableDbContext.RouteTail is null)
                {
                    throw new ShardingCoreInvalidOperationException("db context model is inited before RouteTail set value");
                }
                if (shardingTableDbContext.RouteTail is INoCacheRouteTail)
                {
                    var multiModel = CreateModel(context, conventionSetBuilder, validator);
                    return multiModel;
                }
            }

            int waitSeconds = 3;
            if (context is IShardingModelCacheOption shardingModelCacheOption)
            {
                waitSeconds = shardingModelCacheOption.GetModelCacheLockObjectSeconds();
            }

            var cacheKey = Dependencies.ModelCacheKeyFactory.Create(context);
            if (!_models.TryGetValue(cacheKey, out var model))
            {

                var acquire = Monitor.TryEnter(_syncObject, TimeSpan.FromSeconds(waitSeconds));
                if (!acquire)
                {
                    throw new ShardingCoreInvalidOperationException("cache model timeout");
                }
                try
                {
                    if (!_models.TryGetValue(cacheKey, out  model))
                    {
                        model = new Lazy<IModel>(
                            () => CreateModel(context, conventionSetBuilder, validator),
                            LazyThreadSafetyMode.ExecutionAndPublication);
                        _models.TryAdd(cacheKey, model);
                    }
                }
                finally
                {
                    Monitor.Exit(_syncObject);
                }
            }

            return model.Value;
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
                _models.TryRemove(key, out var x);
            }
            finally
            {
                Monitor.Exit(_syncObject);
            }
        }
    }
}

#endif