using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore.Bootstrappers;
using ShardingCore.Core.QueryRouteManagers.Abstractions;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Logger;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.MergeEngines.ParallelControl;


namespace ShardingCore.Core
{
    public sealed class ShardingRuntimeContext
    {
        private bool isInited = false;
        private object INIT_LOCK = new object();
        private IServiceCollection _serviceMap = new ServiceCollection();

        private IServiceProvider _serviceProvider;
        private IServiceProvider _applicationServiceProvider;


        private ShardingRuntimeContext()
        {
        }

        private static readonly ShardingRuntimeContext _instance = new ShardingRuntimeContext();
        public static ShardingRuntimeContext GetInstance() => _instance;


        public void AddServiceConfig(Action<IServiceCollection> configure)
        {
            CheckIfBuild();
            configure(_serviceMap);
        }

        public void Initialize()
        {
            // var shardingRuntimeModelCacheFactory = _serviceProvider.GetRequiredService<IShardingRuntimeModelCacheFactory>();
            // var cacheKey = shardingRuntimeModelCacheFactory.GetCacheKey(dbContext.GetType());
            // var memoryCache = _serviceProvider.GetRequiredService<IMemoryCache>();
            // if (!memoryCache.TryGetValue(cacheKey, out IShardingRuntimeModel model))
            // {
            //     
            //     // Make sure OnModelCreating really only gets called once, since it may not be thread safe.
            //     var acquire = Monitor.TryEnter(INIT_LOCK, TimeSpan.FromSeconds(waitSeconds));
            //     if (!acquire)
            //     {
            //         throw new ShardingCoreInvalidOperationException("cache model timeout");
            //     }
            //     try
            //     {
            //         if (!cache.TryGetValue(cacheKey, out model))
            //         {
            //             model = CreateModel(
            //                 context, modelCreationDependencies.ConventionSetBuilder, modelCreationDependencies.ModelDependencies);
            //
            //             model = modelCreationDependencies.ModelRuntimeInitializer.Initialize(
            //                 model, designTime, modelCreationDependencies.ValidationLogger);
            //
            //             model = cache.Set(cacheKey, model, new MemoryCacheEntryOptions { Size = size, Priority = priority });
            //         }
            //     }
            //     finally
            //     {
            //         Monitor.Exit(_syncObject);
            //     }
            // }
            if (isInited)
            {
                return;
            }

            lock (INIT_LOCK)
            {

                if (isInited)
                {
                    return;

                }
                _serviceProvider = _serviceMap.BuildServiceProvider();
                _serviceProvider.GetRequiredService<IShardingBootstrapper>().Start();
                isInited = true;
            }
        }

        public void UseLogfactory(ILoggerFactory loggerFactory)
        {
            InternalLoggerFactory.DefaultFactory = loggerFactory;
        }

        public void WithApplicationServiceProvider(IServiceProvider applicationServiceProvider)
        {
            _applicationServiceProvider = applicationServiceProvider;
        }

        private void CheckIfBuild()
        {
            if (isInited)
                throw new InvalidOperationException("sharding runtime already build");
        }
        private void CheckIfNotBuild()
        {
            if (isInited)
                throw new InvalidOperationException("sharding runtime not init");
        }
        
        
          /// <summary>
         /// 创建一个没有依赖注入的对象,但是对象的构造函数参数是已经可以通过依赖注入获取的
         /// </summary>
         /// <param name="serviceType"></param>
         /// <returns></returns>
         /// <exception cref="ArgumentException"></exception>
         public  object CreateInstance(Type serviceType)
         {
             var constructors
                 = serviceType.GetTypeInfo().DeclaredConstructors
                     .Where(c => !c.IsStatic && c.IsPublic)
                     .ToArray();

             if (constructors.Length != 1)
             {
                 throw new ArgumentException(
                     $"type :[{serviceType}] found more than one  declared constructor ");
             }
             var @params = constructors[0].GetParameters().Select(x => GetRequiredService(x.ParameterType))
                 .ToArray();
             return Activator.CreateInstance(serviceType, @params);
         }

          public object GetService(Type serviceType)
          {
              CheckIfNotBuild();
              return _serviceProvider.GetService(serviceType);
          }

          public TService GetService<TService>()
          {
              CheckIfNotBuild();
              return _serviceProvider.GetService<TService>();
          }
          private object GetRequiredService(Type serviceType)
          {
              var service = _serviceProvider?.GetService(serviceType);
              if (service == null)
              {
                  service= _applicationServiceProvider?.GetService(serviceType);
              }

              if (service == null)
              {
                  throw new ShardingCoreInvalidOperationException($"cant unable resolve service:[{serviceType}]");
              }
              return service;
          }

          public IShardingRouteManager GetShardingRouteManager()
          {
              return GetService<IShardingRouteManager>();
          }
          
         public  IShardingEntityConfigOptions<TShardingDbContext> GetRequiredShardingEntityConfigOption<TShardingDbContext>()
             where TShardingDbContext : DbContext, IShardingDbContext
         {
             return (IShardingEntityConfigOptions<TShardingDbContext>)GetRequiredShardingEntityConfigOption(typeof(TShardingDbContext));
         }
         public  IShardingEntityConfigOptions GetRequiredShardingEntityConfigOption(Type shardingDbContextType)
         {
             return (IShardingEntityConfigOptions)GetService(typeof(IShardingEntityConfigOptions<>).GetGenericType0(shardingDbContextType));
         }
    }
}