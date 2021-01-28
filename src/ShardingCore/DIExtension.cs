using System;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge;

namespace ShardingCore
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 13:32:18
* @Email: 326308290@qq.com
*/
    public static class DIExtension
    {
        
        public static IServiceCollection AddShardingCore(this IServiceCollection services)
        {
            services.AddScoped<IStreamMergeContextFactory, StreamMergeContextFactory>();
            services.AddScoped<IRouteRuleEngine, QueryRouteRuleEngines>();
            services.AddScoped<IRoutingRuleEngineFactory, RoutingRuleEngineFactory>();
            return services;
        }
    }
}