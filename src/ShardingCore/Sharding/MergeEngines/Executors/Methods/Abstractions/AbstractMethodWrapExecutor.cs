using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeEngines.Common;
using ShardingCore.Sharding.MergeEngines.Executors.Abstractions;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.Methods.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/7 7:45:07
    /// Email: 326308290@qq.com
    internal abstract class AbstractMethodWrapExecutor<TResult> : AbstractExecutor<RouteQueryResult<TResult>>
    {
        protected AbstractMethodWrapExecutor(StreamMergeContext streamMergeContext) : base(streamMergeContext)
        {
        }

        protected override async Task<ShardingMergeResult<RouteQueryResult<TResult>>> ExecuteUnitAsync(SqlExecutorUnit sqlExecutorUnit, CancellationToken cancellationToken = new CancellationToken())
        {
            var streamMergeContext = GetStreamMergeContext();
            var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
            var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;

            var shardingDbContext = streamMergeContext.CreateDbContext(sqlExecutorUnit.RouteUnit);
            var newQueryable = GetStreamMergeContext().GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);

            var queryResult = await EFCoreQueryAsync(newQueryable, cancellationToken);
            var routeQueryResult = new RouteQueryResult<TResult>(dataSourceName, routeResult, queryResult);
            await streamMergeContext.DbContextDisposeAsync(shardingDbContext);
            return new ShardingMergeResult<RouteQueryResult<TResult>>(null, routeQueryResult);
        }

        protected abstract Task<TResult> EFCoreQueryAsync(IQueryable queryable, CancellationToken cancellationToken = new CancellationToken());
    }
}
