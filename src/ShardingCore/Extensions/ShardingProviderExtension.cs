using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core;
using ShardingCore.Core.ServiceProviders;

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


    }
}
