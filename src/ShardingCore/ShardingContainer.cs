using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources.Abstractions;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Core.VirtualDatabase.VirtualTables;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 02 January 2021 19:37:27
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分片容器全局唯一提供静态依赖注入<code>IServiceProvider</code>
    /// </summary>
    public class ShardingContainer
    {
        private ShardingContainer()
        {

        }

        private static IServiceProvider serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { return serviceProvider ?? throw new ShardingCoreInvalidOperationException("sharding core not start"); }
        }
        /// <summary>
        /// 静态注入
        /// </summary>
        /// <param name="services"></param>
        public static void SetServices(IServiceProvider services)
        {
            if (serviceProvider == null)
                serviceProvider = services;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
        /// <summary>
        /// 获取服务集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetServices<T>()
        {
            return ServiceProvider.GetServices<T>();
        }
        /// <summary>
        /// 根据类型获取服务
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }
        /// <summary>
        /// 创建一个没有依赖注入的对象,但是对象的构造函数参数是已经可以通过依赖注入获取的
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object CreateInstance(Type serviceType)
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
            var @params = constructors[0].GetParameters().Select(x => ServiceProvider.GetService(x.ParameterType))
                .ToArray();
            return Activator.CreateInstance(serviceType, @params);
        }
        /// <summary>
        /// 创建一个没有依赖注入的对象,但是对象的构造函数参数是已经可以通过依赖注入获取并且也存在自行传入的参数,优先判断自行传入的参数
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object CreateInstanceWithInputParams(Type serviceType, params object[] args)
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

            var argIsNotEmpty = args.IsNotEmpty();
            var @params = constructors[0].GetParameters().Select(x =>
                {
                    if (argIsNotEmpty)
                    {
                        var arg = args.FirstOrDefault(o => o.GetType() == x.ParameterType);
                        if (arg != null)
                            return arg;
                    }
                    return ServiceProvider.GetService(x.ParameterType);
                })
                .ToArray();
            return Activator.CreateInstance(serviceType, @params);
        }

        //public static IShardingConfigOption<TShardingDbContext> GetRequiredShardingConfigOption<TShardingDbContext>()
        //    where TShardingDbContext : DbContext, IShardingDbContext
        //{
        //    return (IShardingConfigOption<TShardingDbContext>)GetRequiredShardingConfigOption(typeof(TShardingDbContext));
        //}
        //public static IShardingConfigOption GetRequiredShardingConfigOption(Type shardingDbContextType)
        //{
        //    return (IShardingConfigOption)ServiceProvider.GetService(typeof(IShardingConfigOption<>).GetGenericType0(shardingDbContextType));
        //}
        public static IShardingEntityConfigOptions<TShardingDbContext> GetRequiredShardingEntityConfigOption<TShardingDbContext>()
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            return (IShardingEntityConfigOptions<TShardingDbContext>)GetRequiredShardingEntityConfigOption(typeof(TShardingDbContext));
        }
        public static IShardingEntityConfigOptions GetRequiredShardingEntityConfigOption(Type shardingDbContextType)
        {
            return (IShardingEntityConfigOptions)ServiceProvider.GetService(typeof(IShardingEntityConfigOptions<>).GetGenericType0(shardingDbContextType));
        }

        public static IVirtualDataSourceManager<TShardingDbContext> GetRequiredVirtualDataSourceManager<TShardingDbContext>()
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            return (IVirtualDataSourceManager<TShardingDbContext>)GetRequiredVirtualDataSourceManager(typeof(TShardingDbContext));
        }
        public static IVirtualDataSourceManager GetRequiredVirtualDataSourceManager(Type shardingDbContextType)
        {
            return (IVirtualDataSourceManager)ServiceProvider.GetService(typeof(IVirtualDataSourceManager<>).GetGenericType0(shardingDbContextType));
        }
        public static IVirtualDataSource<TShardingDbContext> GetRequiredCurrentVirtualDataSource<TShardingDbContext>()
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            return GetRequiredVirtualDataSourceManager<TShardingDbContext>().GetCurrentVirtualDataSource() ?? throw new InvalidOperationException("cant resolve CurrentVirtualDataSource");
        }
        public static IVirtualDataSource GetRequiredCurrentVirtualDataSource(Type shardingDbContextType)
        {
            return GetRequiredVirtualDataSourceManager(shardingDbContextType).GetCurrentVirtualDataSource()??throw new InvalidOperationException("cant resolve CurrentVirtualDataSource");
        }

        public static IEntityMetadataManager GetRequiredEntityMetadataManager(Type shardingDbContextType)
        {
            return (IEntityMetadataManager)ServiceProvider.GetService(typeof(IEntityMetadataManager<>).GetGenericType0(shardingDbContextType));
        }

        public static IEntityMetadataManager<TShardingDbContext> GetRequiredEntityMetadataManager<TShardingDbContext>()
            where TShardingDbContext : DbContext, IShardingDbContext
        {
            return (IEntityMetadataManager<TShardingDbContext>)GetRequiredEntityMetadataManager(typeof(TShardingDbContext));
        }

        public static ITrackerManager GetTrackerManager(Type shardingDbContextType)
        {
           return (ITrackerManager)ServiceProvider.GetService(typeof(ITrackerManager<>).GetGenericType0(shardingDbContextType));
        }

        public static IVirtualTableManager GetVirtualTableManager(Type shardingDbContextType)
        {
            return (IVirtualTableManager)ServiceProvider.GetService(typeof(IVirtualTableManager<>).GetGenericType0(shardingDbContextType));
        }
    }
}