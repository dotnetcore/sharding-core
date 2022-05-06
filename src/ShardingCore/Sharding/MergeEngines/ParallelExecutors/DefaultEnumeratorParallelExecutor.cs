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
    internal class DefaultEnumeratorParallelExecutor<TEntity>:AbstractEnumeratorParallelExecutor<TEntity>
    {
        private readonly StreamMergeContext _streamMergeContext;
        private readonly bool _async;

        public DefaultEnumeratorParallelExecutor(StreamMergeContext streamMergeContext,bool async)
        {
            _streamMergeContext = streamMergeContext;
            _async = async;
        }
        public override async Task<ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>> ExecuteAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
             var connectionMode = _streamMergeContext.RealConnectionMode(sqlExecutorUnit.ConnectionMode);
            var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
            var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;
            var (newQueryable, dbContext) = CreateAsyncExecuteQueryable(dataSourceName, routeResult, connectionMode);
            var streamMergeAsyncEnumerator = await AsyncParallelEnumerator(newQueryable, _async, cancellationToken);
            return new ShardingMergeResult<IStreamMergeAsyncEnumerator<TEntity>>(dbContext, streamMergeAsyncEnumerator);
        }
        private (IQueryable<TEntity>, DbContext) CreateAsyncExecuteQueryable(string dsname, TableRouteResult tableRouteResult, ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = _streamMergeContext.CreateDbContext(dsname, tableRouteResult, connectionMode);
            var newQueryable = (IQueryable<TEntity>)_streamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            return (newQueryable, shardingDbContext);
        }
    }
}
