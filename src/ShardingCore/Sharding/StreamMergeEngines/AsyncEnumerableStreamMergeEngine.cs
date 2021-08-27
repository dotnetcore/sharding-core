using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine;
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
    public class AsyncEnumerableStreamMergeEngine<T> : IAsyncEnumerable<T>, IEnumerable<T>, IDisposable
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly ICollection<DbContext> _parllelDbbContexts;

        public AsyncEnumerableStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
            _parllelDbbContexts = new LinkedList<DbContext>();
        }


#if !EFCORE2

        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetAsyncEnumerator();
            await enumator.MoveNextAsync();
            return enumator;
        }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return GetShardingEnumerator();
        }
#endif

#if EFCORE2
        private async Task<IAsyncEnumerator<T>> GetAsyncEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsAsyncEnumerable().GetEnumerator();
            await enumator.MoveNext();
            return enumator;
        }
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return GetShardingEnumerator();
        }
#endif

        private IQueryable<T> CreateAsyncExecuteQueryable(RouteResult routeResult,int routeCount)
        {
            var shardingDbContext = _mergeContext.CreateDbContext(routeResult);
            var useOriginal = routeCount>1;
            _parllelDbbContexts.Add(shardingDbContext);
            var newQueryable = (IQueryable<T>)(useOriginal?_mergeContext.GetReWriteQueryable():_mergeContext.GetOriginalQueryable())
                .ReplaceDbContextQueryable(shardingDbContext);
            return newQueryable;
        }

        private IAsyncEnumerator<T> GetShardingEnumerator()
        {
            var tableResult = _mergeContext.GetRouteResults();
            var routeCount = tableResult.Count();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var newQueryable = CreateAsyncExecuteQueryable(routeResult, routeCount);

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
            if (routeCount>1&&_mergeContext.HasSkipTake())
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
            var routeCount = tableResult.Count();
            var enumeratorTasks = tableResult.Select(routeResult =>
            {
                return Task.Run(() =>
                {
                    try
                    {
                        var newQueryable = CreateAsyncExecuteQueryable(routeResult, routeCount);

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
            if (routeCount > 1 && _mergeContext.HasSkipTake())
                return new PaginationStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
            if (_mergeContext.HasGroupQuery())
                return new MultiAggregateOrderStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
            return new MultiOrderStreamMergeEnumerator<T>(_mergeContext, streamEnumerators);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_parllelDbbContexts.IsNotEmpty())
            {
                _parllelDbbContexts.ForEach(o =>
                {
                    try
                    {
                        o.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }
    }
}