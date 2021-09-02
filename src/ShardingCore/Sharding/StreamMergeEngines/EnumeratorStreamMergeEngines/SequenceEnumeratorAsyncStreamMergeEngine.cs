using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.ShardingPage.Abstractions;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.EnumeratorStreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 16:29:06
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class SequenceEnumeratorAsyncStreamMergeEngine<TEntity> : AbstractEnumeratorAsyncStreamMergeEngine<TEntity>
    {
        private IShardingPageManager _shardingPageManager;
        private IVirtualTableManager _virtualTableManager;
        public SequenceEnumeratorAsyncStreamMergeEngine(StreamMergeContext<TEntity> streamMergeContext) : base(streamMergeContext)
        {
            _shardingPageManager = ShardingContainer.GetService<IShardingPageManager>();
            _virtualTableManager = ShardingContainer.GetService<IVirtualTableManager>();
        }

        public override IStreamMergeAsyncEnumerator<TEntity>[] GetDbStreamMergeAsyncEnumerators()
        {
            var routeQueryResults = _shardingPageManager.Current.RouteQueryResults.ToList();
            if (routeQueryResults.Any(o => o.RouteResult.ReplaceTables.Count(p=>p.IsShardingTable())!=1)
                || routeQueryResults.HasDifference(o=>o.RouteResult.ReplaceTables.First().EntityType))
                throw new InvalidOperationException($"error sharding page:[{StreamMergeContext.GetOriginalQueryable().Expression.ShardingPrint()}]");
            var shardingEntityType = routeQueryResults[0].RouteResult.ReplaceTables.FirstOrDefault(o=>o.IsShardingTable()).EntityType;
            var virtualTable = _virtualTableManager.GetVirtualTable(StreamMergeContext.GetShardingDbContext().ShardingDbContextType, shardingEntityType);
            if (!virtualTable.EnablePagination)
            {
                throw new ShardingCoreException("not support Sequence enumerator");
            }

            if (base.StreamMergeContext.Orders.IsEmpty())
            {
                var append = virtualTable.PaginationMetadata.PaginationConfigs.FirstOrDefault(o=>o.AppendIfOrderNone);
                if (append != null)
                {
                    StreamMergeContext.GetOriginalQueryable().OrderBy("")
                }
            }


            var tableResult = StreamMergeContext.RouteResults;
            var routeCount = tableResult.Count();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var newQueryable = CreateAsyncExecuteQueryable(routeResult, routeCount);

                        var asyncEnumerator = await DoGetAsyncEnumerator(newQueryable);
                        return new StreamMergeAsyncEnumerator<TEntity>(asyncEnumerator);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).GetAwaiter().GetResult();
            return streamEnumerators;
        }

        public override IQueryable<TEntity> CreateAsyncExecuteQueryable(RouteResult routeResult, int routeCount)
        {
            var shardingDbContext = StreamMergeContext.CreateDbContext(routeResult);
            DbContextQueryStore.TryAdd(routeResult, shardingDbContext);
            var newQueryable = (IQueryable<TEntity>)(_multiRouteQuery ? StreamMergeContext.GetReWriteQueryable() : StreamMergeContext.GetOriginalQueryable())
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        public override IStreamMergeAsyncEnumerator<TEntity> GetStreamMergeAsyncEnumerator(IStreamMergeAsyncEnumerator<TEntity>[] streamsAsyncEnumerators)
        {
            if (_multiRouteQuery && StreamMergeContext.HasSkipTake())
                return new PaginationStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            if (StreamMergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<TEntity>(StreamMergeContext, streamsAsyncEnumerators);
        }
    }
}
