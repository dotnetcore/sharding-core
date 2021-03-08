using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.Internal.RoutingRuleEngines;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Core.Internal.StreamMerge.Enumerators;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Extensions;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Core.Internal.StreamMerge.GenericMerges
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 29 January 2021 15:40:15
* @Email: 326308290@qq.com
*/
    internal class GenericStreamMergeEngine<T> : IStreamMergeEngine<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly ICollection<DbContext> _parallelDbContexts;

        public GenericStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
            _parallelDbContexts = new LinkedList<DbContext>();
        }

        public static GenericStreamMergeEngine<T> Create<T>(StreamMergeContext<T> mergeContext)
        {
            return new GenericStreamMergeEngine<T>(mergeContext);
        }

        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(string connectKey,IQueryable<T> newQueryable, RouteResult routeResult)
        {
            using (var scope = _mergeContext.CreateScope())
            {
                scope.ShardingAccessor.ShardingContext = ShardingContext.Create(connectKey,routeResult);
#if !EFCORE2
                var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
                await enumator.MoveNextAsync();
#endif
#if EFCORE2
                var enumator = newQueryable.AsAsyncEnumerable().GetEnumerator();
                await enumator.MoveNext();
#endif
                return enumator;
            }
        }

        public async Task<IStreamMergeAsyncEnumerator<T>> GetStreamEnumerator()
        {
            var dataSourceResult = _mergeContext.GetDataSourceRoutingResult();
            var enumeratorTasks = dataSourceResult.IntersectConfigs.SelectMany(connectKey =>
            {
                var tableResult = _mergeContext.GetRouteResults(connectKey);
                return tableResult.Select(routeResult =>
                {
                    return Task.Run(async () =>
                    {
                        var shardingDbContext = _mergeContext.CreateDbContext(connectKey);
                        _parallelDbContexts.Add(shardingDbContext);
                        var newQueryable = (IQueryable<T>) _mergeContext.GetReWriteQueryable()
                            .ReplaceDbContextQueryable(shardingDbContext);

                        var asyncEnumerator = await GetAsyncEnumerator(connectKey,newQueryable, routeResult);
                        return new StreamMergeAsyncEnumerator<T>(asyncEnumerator);
                    });
                });
            }).ToArray();

            var streamEnumerators = await Task.WhenAll(enumeratorTasks);
            if (_mergeContext.HasSkipTake())
                return new NoPaginationStreamMergeEnumerator<T>(_mergeContext,streamEnumerators );
            if(_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        }


        public void Dispose()
        {
            _parallelDbContexts.ForEach(o => o.Dispose());
        }
    }
}