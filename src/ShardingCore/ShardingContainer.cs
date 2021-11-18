using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

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
            serviceProvider = services;
        }

        public static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
        public static IEnumerable<T> GetServices<T>()
        {
            return ServiceProvider.GetServices<T>();
        }
        public static object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

    }
}