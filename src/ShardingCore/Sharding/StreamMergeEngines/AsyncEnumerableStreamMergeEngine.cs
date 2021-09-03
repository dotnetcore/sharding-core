using ShardingCore.Sharding.ShardingQueryExecutors;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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