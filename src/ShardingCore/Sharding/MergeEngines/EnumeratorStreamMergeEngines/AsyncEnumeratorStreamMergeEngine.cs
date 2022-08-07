using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.TrackerEnumerators;
using ShardingCore.Sharding.MergeEngines.Enumerables;
using ShardingCore.Sharding.ShardingQueryExecutors;

/*
* @Author: xjm
* @Description: 迭代聚合流式引擎
* @Date: Saturday, 14 August 2021 22:07:28
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Sharding.MergeEngines.EnumeratorStreamMergeEngines
{
    internal class AsyncEnumeratorStreamMergeEngine<T> : IAsyncEnumerable<T>, IEnumerable<T>
    {
        private readonly StreamMergeContext _mergeContext;

        public AsyncEnumeratorStreamMergeEngine(StreamMergeContext mergeContext)
        {
            _mergeContext = mergeContext;
        }


#if !EFCORE2
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_mergeContext.TryPrepareExecuteContinueQuery(() => new EmptyQueryEnumerator<T>(),
                    out var emptyQueryEnumerator))
            {
                return emptyQueryEnumerator;
            }
            var asyncEnumerator = EnumeratorStreamMergeEngineFactory<T>.Create(_mergeContext).GetStreamEnumerable()
                .GetAsyncEnumerator(cancellationToken);

            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new AsyncTrackerEnumerator<T>(_mergeContext.GetShardingDbContext(), asyncEnumerator);
            }

            return asyncEnumerator;
        }
#endif

#if EFCORE2
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            if (!_mergeContext.TryPrepareExecuteContinueQuery(() => new EmptyQueryEnumerator<T>(),out var emptyQueryEnumerator))
                return emptyQueryEnumerator;
            var asyncEnumerator = ((IAsyncEnumerable<T>)EnumeratorStreamMergeEngineFactory<T>.Create(_mergeContext).GetStreamEnumerable())
                .GetEnumerator();
            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new AsyncTrackerEnumerator<T>(_mergeContext.GetShardingDbContext(), asyncEnumerator);
            }
            return asyncEnumerator;
        }
#endif


        public IEnumerator<T> GetEnumerator()
        {
            if (!_mergeContext.TryPrepareExecuteContinueQuery(() => new EmptyQueryEnumerator<T>(),out var emptyQueryEnumerator))
                return emptyQueryEnumerator;
            var enumerator = ((IEnumerable<T>)EnumeratorStreamMergeEngineFactory<T>.Create(_mergeContext).GetStreamEnumerable())
                .GetEnumerator();

            if (_mergeContext.IsUseShardingTrack(typeof(T)))
            {
                return new TrackerEnumerator<T>(_mergeContext.GetShardingDbContext(), enumerator);
            }
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}