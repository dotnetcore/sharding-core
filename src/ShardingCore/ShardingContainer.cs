using System;
using Microsoft.Extensions.DependencyInjection;

namespace ShardingCore
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 02 January 2021 19:37:27
* @Email: 326308290@qq.com
*/
    public class ShardingContainer
    {
        private ShardingContainer()
        {
            
        }
        public static IServiceProvider Services { get; private set; }
        /// <summary>
        /// 静态注入
        /// </summary>
        /// <param name="services"></param>
        public static void SetServices(IServiceProvider services)
        {
            Services = services;
        }

        public static T GetService<T>()
        {
            return Services.GetService<T>();
        }
        public static object GetService(Type serviceType)
        {
            return Services.GetService(serviceType);
        }
    }
}