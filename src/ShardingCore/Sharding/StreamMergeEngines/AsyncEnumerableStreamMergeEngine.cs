using ShardingCore.Sharding.ShardingQueryExecutors;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines.TrackerEnumerators;

namespace ShardingCore.Sharding.StreamMergeEngines
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 14 August 2021 22:07:28
    * @Email: 326308290@qq.com
    */
    public class AsyncEnumerableStreamMergeEngine<TShardingDbContext,T> : IAsyncEnumerable<T>, IEnumerable<T>
    where  TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public AsyncEnumerableStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }


#if !EFCORE2
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            var asyncEnumerator = new EnumeratorShardingQueryExecutor<TShardingDbContext,T>(_mergeContext).ExecuteAsync(cancellationToken)
                .GetAsyncEnumerator(cancellationToken);

            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new AsyncTrackerEnumerator<T>(_mergeContext, asyncEnumerator);
            }

            return asyncEnumerator;
        }
#endif

#if EFCORE2
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            var asyncEnumerator = ((IAsyncEnumerable<T>)new EnumeratorShardingQueryExecutor<TShardingDbContext,T>(_mergeContext).ExecuteAsync())
                .GetEnumerator();
            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new AsyncTrackerEnumerator<T>(_mergeContext, asyncEnumerator);
            }
            return asyncEnumerator;
        }
#endif


        public IEnumerator<T> GetEnumerator()
        {
            var enumerator = ((IEnumerable<T>)new EnumeratorShardingQueryExecutor<TShardingDbContext,T>(_mergeContext).ExecuteAsync())
                .GetEnumerator();

            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new TrackerEnumerator<T>(_mergeContext, enumerator);
            }
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}