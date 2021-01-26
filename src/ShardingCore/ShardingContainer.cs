using System;

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
        public static IServiceProvider Services { get; private set; }
        /// <summary>
        /// 静态注入
        /// </summary>
        /// <param name="services"></param>
        public static void SetServices(IServiceProvider services)
        {
            Services = services;
        }
    }
}