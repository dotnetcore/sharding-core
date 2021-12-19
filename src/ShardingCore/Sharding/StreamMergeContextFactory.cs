using System.Diagnostics;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Sharding.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.DataSourceRoutes.RouteRuleEngine;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding.ShardingExecutors.Abstractions;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Thursday, 28 January 2021 16:52:43
    * @Email: 326308290@qq.com
    */
    public class StreamMergeContextFactory<TShardingDbContext> : IStreamMergeContextFactory<TShardingDbContext> where TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly IRouteTailFactory _routeTailFactory;

        public StreamMergeContextFactory(IRouteTailFactory routeTailFactory)
        {
            _routeTailFactory = routeTailFactory;
        }
        public StreamMergeContext<T> Create<T>(IMergeQueryCompilerContext mergeQueryCompilerContext)
        {
            return new StreamMergeContext<T>(mergeQueryCompilerContext, _routeTailFactory);
        }
    }
}