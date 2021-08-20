using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Enumerators;
using ShardingCore.Sharding.Enumerators.StreamMergeAsync;
using ShardingCore.Sharding.Enumerators.StreamMergeSync;

#if EFCORE2
using Microsoft.EntityFrameworkCore.Extensions.Internal;
#endif

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 22:07:28
    * @Email: 326308290@qq.com
    */
    public class AsyncEnumerableStreamMergeEngine<T> : IAsyncEnumerable<T>, IEnumerable<T>
    {

        private readonly StreamMergeContext<T> _mergeContext;

        public AsyncEnumerableStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }


#if !EFCORE2

        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }
#endif
#if EFCORE2

        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetEnumerator();
            await enumator.MoveNext();
            return enumator;
        }
#endif
#if !EFCORE2

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return GetShardingEnumerator();
        }
#endif

#if EFCORE2
        
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return GetShardingEnumerator();
        }
#endif

        private IAsyncEnumerator<T> GetShardingEnumerator()
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
                        var shardingDbContext = _mergeContext.CreateDbContext(tail);
                        var newQueryable = (IQueryable<T>)_mergeContext.GetReWriteQueryable()
                            .ReplaceDbContextQueryable(shardingDbContext);

                        var asyncEnumerator = await GetAsyncEnumerator(newQueryable);
                        return new StreamMergeAsyncEnumerator<T>(asyncEnumerator);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).GetAwaiter().GetResult();
            if (_mergeContext.HasSkipTake())
                return new PaginationStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            if (_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        }


        private IEnumerator<T> GetEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsEnumerable().GetEnumerator();
            enumator.MoveNext();
            return enumator;
        }
        public IEnumerator<T> GetEnumerator()
        {
            var tableResult = _mergeContext.GetRouteResults();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                if (routeResult.ReplaceTables.Count > 1)
                    throw new ShardingCoreException("route found more than 1 table name s");
                var tail = string.Empty;
                if (routeResult.ReplaceTables.Count == 1)
                    tail = routeResult.ReplaceTables.First().Tail;

                return Task.Run(() =>
               {
                   try
                   {

                       var shardingDbContext = _mergeContext.CreateDbContext(tail);
                       var newQueryable = (IQueryable<T>)_mergeContext.GetReWriteQueryable()
                               .ReplaceDbContextQueryable(shardingDbContext);

                       var enumerator = GetEnumerator(newQueryable);
                       return new StreamMergeEnumerator<T>(enumerator);
                   }
                   catch (Exception e)
                   {
                       Console.WriteLine(e);
                       throw;
                   }
               });
            }).ToArray();

            var streamEnumerators = Task.WhenAll(enumeratorTasks).GetAwaiter().GetResult();
            if (_mergeContext.HasSkipTake())
                return new PaginationStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
            if (_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
