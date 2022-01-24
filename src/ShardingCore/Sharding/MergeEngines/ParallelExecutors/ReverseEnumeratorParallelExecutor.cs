using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.MergeEngines.Abstractions;
using ShardingCore.Sharding.MergeEngines.Common;

namespace ShardingCore.Sharding.MergeEngines.ParallelExecutors
{
    internal class ReverseEnumeratorParallelExecutor<TEntity>:AbstractEnumeratorParallelExecutor<TEntity>
    {
        private readonly StreamMergeContext<TEntity> _streamMergeContext;
        private readonly IOrderedQueryable<TEntity> _reverseOrderQueryable;
        private readonly bool _async;

        public ReverseEnumeratorParallelExecutor(StreamMergeContext<TEntity> streamMergeContext,IOrderedQueryable<TEntity> reverseOrderQueryable, bool async)
        {
            _streamMergeContext = streamMergeContext;
            _reverseOrderQueryable = reverseOrderQueryable;
            _async = async;
        }
        public override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var connectionMode = _streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
            var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;
            var (newQueryable, dbContext) =
                CreateAsyncExecuteQueryable(dataSourceName, _reverseOrderQueryable, routeResult, connectionMode);
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(dbContext,
                streamMergeAsyncEnumerator);
        }
        private (IQueryable<TEntity>, DbContext) CreateAsyncExecuteQueryable(string dsname, IQueryable<TEntity> reverseOrderQueryable, TableRouteResult tableRouteResult, ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = _streamMergeContext.CreateDbContext(dsname, tableRouteResult, connectionMode);
            var newQueryable = (IQueryable<TEntity>)reverseOrderQueryable
                .ReplaceDbContextQueryable(shardingDbContext);
            return (newQueryable, shardingDbContext);
        }
    }
}
