using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.Visitors;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.DbContexts;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 25 January 2021 11:38:27
* @Email: 326308290@qq.com
*/
    internal class StreamMergeContext<T>
    {
        private readonly IShardingParallelDbContextFactory _shardingParallelDbContextFactory;
        private readonly IShardingScopeFactory _shardingScopeFactory;
        public IQueryable<T> Source { get; }
        public IEnumerable<RouteResult> RouteResults { get; }
        public int? Skip { get; private set; }
        public int? Take { get; private set; }
        public IEnumerable<PropertyOrder> Orders { get; private set; }

        public StreamMergeContext(IQueryable<T> source,IEnumerable<RouteResult> routeResults,
            IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            _shardingScopeFactory = shardingScopeFactory;
            Source = source;
            RouteResults = routeResults;
            var extraEntry = source.GetExtraEntry();
            Skip = extraEntry.Skip;
            Take = extraEntry.Take;
            Orders = extraEntry.Orders ?? Enumerable.Empty<PropertyOrder>();
        }

        public DbContext CreateDbContext()
        {
            return _shardingParallelDbContextFactory.Create(string.Empty);
        }

        public ShardingScope CreateScope()
        {
            return _shardingScopeFactory.CreateScope();
        }

    }
}