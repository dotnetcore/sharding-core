using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.MergeEngines.Abstractions.StreamMerge;

namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines.EnumeratorAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:16:12
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal class DefaultShardingEnumeratorAsyncStreamMergeEngine<TShardingDbContext,TEntity> :AbstractEnumeratorStreamMergeEngine<TEntity>
        where TShardingDbContext : DbContext, IShardingDbContext
    {
        public DefaultShardingEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetRouteQueryStreamMergeAsyncEnumerators(bool async, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var defaultSqlRouteUnits = GetDefaultSqlRouteUnits();
            var enumeratorTasks = GetDataSourceGroupAndExecutorGroup<IStreamMergeAsyncEnumerator<TEntity>>(defaultSqlRouteUnits,
                async sqlExecutorUnit =>
                {
                    var connectionMode = GetStreamMergeContext().RealConnectionMode(sqlExecutorUnit.ConnectionMode);
                    var dataSourceName = sqlExecutorUnit.RouteUnit.DataSourceName;
                    var routeResult = sqlExecutorUnit.RouteUnit.TableRouteResult;
                    var (newQueryable,dbContext) = CreateAsyncExecuteQueryable(dataSourceName, routeResult, connectionMode);
                    return await AsyncParallelEnumerator(newQueryable, async,connectionMode, cancellationToken)
                        .ReleaseConnectionAsync(dbContext, connectionMode);
                }, cancellationToken);

            var streamEnumerators = Task.WhenAll(enumeratorTasks).WaitAndUnwrapException().SelectMany(o=>o).ToArray();
            return streamEnumerators;
        }

        private (IQueryable<TEntity>,DbContext) CreateAsyncExecuteQueryable(string dsname,TableRouteResult tableRouteResult,ConnectionModeEnum connectionMode)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(dsname,tableRouteResult, connectionMode);
            var newQueryable = (IQueryable<TEntity>)StreamMergeContext.GetReWriteQueryable()
                .ReplaceDbContextQueryable(shardingDbContext);
            return (newQueryable,shardingDbContext);
        }

        public override IStreamMergeAsyncEnumerator<TEntity> CombineStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (StreamMergeContext.IsPaginationQuery())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
