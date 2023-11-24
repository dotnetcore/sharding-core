using System.Threading;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;

namespace ShardingCore.Core.DbContextCreator
{
    
    public class RouteTailContextHelper
    {
    
        private static AsyncLocal<IRouteTail> _shardingRouteContext = new AsyncLocal<IRouteTail>();

        /// <summary>
        /// sharding route context use in using code block
        /// </summary>
        public static IRouteTail RouteTail
        {
            get => _shardingRouteContext.Value;
            set => _shardingRouteContext.Value = value;
        }
    }
}
