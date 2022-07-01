using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;

namespace ShardingCore.Extensions
{
    
    public static class ShardingProviderExtension
    {
        /// <summary>
        /// 创建一个没有依赖注入的对象,但是对象的构造函数参数是已经可以通过依赖注入获取的
        /// </summary>
        /// <param name="shardingProvider"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object CreateInstance(this IShardingProvider shardingProvider,Type serviceType)
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
            var @params = constructors[0].GetParameters().Select(x => shardingProvider.GetService(x.ParameterType))
                .ToArray();
            return Activator.CreateInstance(serviceType, @params);
        }

        public static object GetRequiredService(this IShardingProvider shardingProvider,Type serviceType)
        {
            var service = shardingProvider.GetService(serviceType);
            if (service == null)
            {
                throw new ArgumentNullException($"cant resolve {serviceType.FullName}");
            }

            return service;
        }

        public static TService GetService<TService>(this IShardingProvider shardingProvider)
        {
            return (TService)shardingProvider.GetService(typeof(TService));
        }

        public static TService GetRequiredService<TService>(this IShardingProvider shardingProvider)
        {
            return (TService)shardingProvider.GetRequiredService(typeof(TService));
        }
    }
}
