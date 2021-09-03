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
using ShardingCore.Sharding.ShardingQueryExecutors;

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
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new EnumeratorShardingQueryExecutor<T>(_mergeContext).ExecuteAsync(cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
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
        


        private IEnumerator<T> GetEnumerator(IQueryable<T> newQueryable)
        {
            var enumator = newQueryable.AsEnumerable().GetEnumerator();
            enumator.MoveNext();
            return enumator;
        }

        public IEnumerator<T> GetEnumerator()
        {

            return new EnumeratorShardingQueryExecutor<T>(_mergeContext).ExecuteAsync()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}