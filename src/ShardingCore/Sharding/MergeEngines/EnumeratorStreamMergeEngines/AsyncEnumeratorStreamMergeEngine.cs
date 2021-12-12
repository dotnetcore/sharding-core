using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.TrackerEnumerators;
using ShardingCore.Sharding.ShardingQueryExecutors;

/*
* @Author: xjm
* @Description: 迭代聚合流式引擎
* @Date: Saturday, 14 August 2021 22:07:28
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines
{
    internal class AsyncEnumeratorStreamMergeEngine<TShardingDbContext,T> : IAsyncEnumerable<T>, IEnumerable<T>
    where  TShardingDbContext:DbContext,IShardingDbContext
    {
        private readonly StreamMergeContext<T> _mergeContext;

        public AsyncEnumeratorStreamMergeEngine(StreamMergeContext<T> mergeContext)
        {
            _mergeContext = mergeContext;
        }


#if !EFCORE2
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var asyncEnumerator = EnumeratorStreamMergeEngineFactory<TShardingDbContext,T>.Create(_mergeContext).GetMergeEngine()
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
            var asyncEnumerator = ((IAsyncEnumerable<T>)EnumeratorStreamMergeEngineFactory<TShardingDbContext,T>.Create(_mergeContext).GetMergeEngine())
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
            var enumerator = ((IEnumerable<T>)EnumeratorStreamMergeEngineFactory<TShardingDbContext,T>.Create(_mergeContext).GetMergeEngine())
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