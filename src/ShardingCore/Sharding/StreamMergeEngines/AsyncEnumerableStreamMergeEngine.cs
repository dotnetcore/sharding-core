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

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 22:07:28
    * @Email: 326308290@qq.com
    */
    public class AsyncEnumerableStreamMergeEngine<T> :IAsyncEnumerable<T>,IEnumerable<T>
    {

        private readonly StreamMergeContext<T> _mergeContext;

        public AsyncEnumerableStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }

        //public static IStreamMergeEngine<T> Create<T>(StreamMergeContext<T> mergeContext)
        //{
        //    return new AsyncEnumerableStreamMergeEngine<T>(mergeContext);
        //}
        //public async Task<IStreamMergeAsyncEnumerator<T>> GetAsyncEnumerator()
        //{
        //    var tableResult = _mergeContext.GetRouteResults();
        //    var enumeratorTasks = tableResult.Select(routeResult =>
        //     {
        //         if (routeResult.ReplaceTables.Count > 1)
        //             throw new ShardingCoreException("route found more than 1 table name s");
        //         var tail = string.Empty;
        //         if (routeResult.ReplaceTables.Count == 1)
        //             tail = routeResult.ReplaceTables.First().Tail;

        //         return Task.Run(async () =>
        //         {
        //             try
        //             {
        //                 //using (var scope = _mergeContext.CreateScope())
        //                 //{
        //                 //var shardingContext = ShardingContext.Create(routeResult);
        //                 //scope.ShardingAccessor.ShardingContext = shardingContext;

        //                 var shardingDbContext = _mergeContext.CreateDbContext(tail);
        //                 var newQueryable = (IQueryable<T>) _mergeContext.GetReWriteQueryable()
        //                         .ReplaceDbContextQueryable(shardingDbContext);

        //                     var asyncEnumerator = await GetAsyncEnumerator(newQueryable);
        //                     return new StreamMergeAsyncEnumerator<T>(asyncEnumerator);
        //                 //}
        //             }
        //             catch (Exception e)
        //             {
        //                 Console.WriteLine(e);
        //                 throw;
        //             }
        //         });
        //     }).ToArray();

        //    var streamEnumerators = await Task.WhenAll(enumeratorTasks);
        //    if (_mergeContext.HasSkipTake())
        //        return new PaginationStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        //    if (_mergeContext.HasGroupQuery())
        //        return new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        //    return new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        //}
        //public IStreamMergeEnumerator<T> GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}



        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
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
                        //using (var scope = _mergeContext.CreateScope())
                        //{
                        //var shardingContext = ShardingContext.Create(routeResult);
                        //scope.ShardingAccessor.ShardingContext = shardingContext;

                        var shardingDbContext = _mergeContext.CreateDbContext(tail);
                        var newQueryable = (IQueryable<T>)_mergeContext.GetReWriteQueryable()
                                .ReplaceDbContextQueryable(shardingDbContext);

                        var asyncEnumerator = await GetAsyncEnumerator(newQueryable);
                        return new StreamMergeAsyncEnumerator<T>(asyncEnumerator);
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
            }).ToArray();

            var streamEnumerators =  Task.WhenAll(enumeratorTasks).GetAwaiter().GetResult();
            if (_mergeContext.HasSkipTake())
                return new PaginationStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            if (_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, streamEnumerators);
        }

        private IEnumerator<T>  GetEnumerator(IQueryable<T> newQueryable)
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

                return Task.Run( () =>
                {
                    try
                    {
                        //using (var scope = _mergeContext.CreateScope())
                        //{
                        //var shardingContext = ShardingContext.Create(routeResult);
                        //scope.ShardingAccessor.ShardingContext = shardingContext;

                        var shardingDbContext = _mergeContext.CreateDbContext(tail);
                        var newQueryable = (IQueryable<T>)_mergeContext.GetReWriteQueryable()
                                .ReplaceDbContextQueryable(shardingDbContext);

                        var enumerator =  GetEnumerator(newQueryable);
                        return new StreamMergeEnumerator<T>(enumerator);
                        //}
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
