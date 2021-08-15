using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.ShardingAccessors;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;

namespace ShardingCore.Sharding
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 22:07:28
    * @Email: 326308290@qq.com
    */
    public class GenericStreamMergeEngine<T> : IStreamMergeEngine<T>,IDisposable
    {

        private readonly StreamMergeContext<T> _mergeContext;
        private readonly ICollection<DbContext> _parallelDbContexts;

        public GenericStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
            _parallelDbContexts = new LinkedList<DbContext>();
        }

        public static IStreamMergeEngine<T> Create<T>(StreamMergeContext<T> mergeContext)
        {
            return new GenericStreamMergeEngine<T>(mergeContext);
        }
        public async Task<IStreamMergeAsyncEnumerator<T>> GetAsyncEnumerator()
        {
            var tableResult = _mergeContext.GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
             {
                 if (routeResult.ReplaceTables.Count > 1)
                     throw new ShardingCoreException("route found more than 1 table name s");
                 var tail = string.Empty;
                 if (routeResult.ReplaceTables.Count == 1)
                     tail = routeResult.ReplaceTables.First().Tail;

                 return Task.Run(async () =>
                 {
                     try
                     {

                         using (var scope = _mergeContext.CreateScope())
                         {
                             var shardingContext = ShardingContext.Create(routeResult);
                             scope.ShardingAccessor.ShardingContext = shardingContext;

                             var shardingDbContext = _mergeContext.CreateDbContext(tail);
                             _parallelDbContexts.Add(shardingDbContext);
                             var newQueryable = (IQueryable<T>) _mergeContext.GetReWriteQueryable()
                                 .ReplaceDbContextQueryable(shardingDbContext);

                             var asyncEnumerator = await GetAsyncEnumerator(newQueryable);
                             return new StreamMergeAsyncEnumerator<T>(asyncEnumerator);
                         }
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine(e);
                         throw;
                     }
                 });
             }).ToArray();

            var streamEnumerators = await Task.WhenAll(enumeratorTasks);
            if (_mergeContext.HasSkipTake())
                return new PaginationStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            if (_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        }

        public Task<IStreamMergeEnumerator<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }



        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }

        public void Dispose()
        {
            _parallelDbContexts.ForEach(o => o.Dispose());
        }
    }
}
