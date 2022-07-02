using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

namespace ShardingCore.Extensions
{
    
    public static class ShardingRouteConfigOptionsExtension
    {
    
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public static void AddShardingDataSourceRoute<TRoute>(this IShardingRouteConfigOptions options) where TRoute : IVirtualDataSourceRoute
        {
            var routeType = typeof(TRoute);
            options.AddShardingDataSourceRoute(routeType);
        }
        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public static void AddShardingTableRoute<TRoute>(this IShardingRouteConfigOptions options) where TRoute : IVirtualTableRoute
        {
            var routeType = typeof(TRoute);
            options.AddShardingTableRoute(routeType);
        }
    }
}
