using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge.ReWrite;
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
        private readonly IQueryable<T> _source;
        private readonly IQueryable<T> _reWriteSource;
        public IEnumerable<RouteResult> RouteResults { get; }
        public int? Skip { get; private set; }
        public int? Take { get; private set; }
        public IEnumerable<PropertyOrder> Orders { get; private set; }

        public StreamMergeContext(IQueryable<T> source,IEnumerable<RouteResult> routeResults,
            IShardingParallelDbContextFactory shardingParallelDbContextFactory,IShardingScopeFactory shardingScopeFactory)
        {
            _shardingParallelDbContextFactory = shardingParallelDbContextFactory;
            _shardingScopeFactory = shardingScopeFactory;
            _source = source;
            RouteResults = routeResults;
            var reWriteResult = new ReWriteEngine<T>(source).ReWrite();
            Skip = reWriteResult.Skip;
            Take = reWriteResult.Take;
            Orders = reWriteResult.Orders ?? Enumerable.Empty<PropertyOrder>();
            _reWriteSource = reWriteResult.ReWriteQueryable;
        }

        public DbContext CreateDbContext()
        {
            return _shardingParallelDbContextFactory.Create(string.Empty);
        }

        public ShardingScope CreateScope()
        {
            return _shardingScopeFactory.CreateScope();
        }

        public IQueryable<T> GetReWriteQueryable()
        {
            return _reWriteSource;
        }
        public IQueryable<T> GetOriginalQueryable()
        {
            return _source;
        }

    }
}