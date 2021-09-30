using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
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
        public static IEnumerable<T> GetServices<T>()
        {
            return Services.GetServices<T>();
        }
        public static object GetService(Type serviceType)
        {
            return Services.GetService(serviceType);
        }
        
    }
}