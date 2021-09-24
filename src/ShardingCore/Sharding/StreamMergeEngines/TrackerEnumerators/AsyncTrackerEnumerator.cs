using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Sharding.StreamMergeEngines.TrackerEnumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/23 22:57:11
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
#if !EFCORE2

    public class AsyncTrackerEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _streamMergeContext;
        private readonly IAsyncEnumerator<T> _asyncEnumerator;

        public AsyncTrackerEnumerator(StreamMergeContext<T> streamMergeContext, IAsyncEnumerator<T> asyncEnumerator)
        {
            _streamMergeContext = streamMergeContext;
            _asyncEnumerator = asyncEnumerator;
        }
        public ValueTask DisposeAsync()
        {
            return _asyncEnumerator.DisposeAsync();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return _asyncEnumerator.MoveNextAsync();
        }

        public T Current => GetCurrent();
        private T GetCurrent()
        {
            var current = _asyncEnumerator.Current;
            if (current != null)
            {
                var c = (object)current;
                var genericDbContext = _streamMergeContext.GetShardingDbContext().CreateGenericDbContext(c);
                var attachedEntity = genericDbContext.GetAttachedEntity(c);
                if (attachedEntity==null)
                    genericDbContext.Attach(current);
                else
                {
                    return (T)attachedEntity;
                }
            }

            return current;
        }
    }
#endif

#if EFCORE2

    public class AsyncTrackerEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _streamMergeContext;
        private readonly IAsyncEnumerator<T> _asyncEnumerator;

        public AsyncTrackerEnumerator(StreamMergeContext<T> streamMergeContext, IAsyncEnumerator<T> asyncEnumerator)
        {
            _streamMergeContext = streamMergeContext;
            _asyncEnumerator = asyncEnumerator;
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return _asyncEnumerator.MoveNext(cancellationToken);
        }

        public T Current => GetCurrent();
        private T GetCurrent()
        {
            var current = _asyncEnumerator.Current;
            if (current != null)
            {
                var c = (object)current;
                var genericDbContext = _streamMergeContext.GetShardingDbContext().CreateGenericDbContext(c);
                var attachedEntity = genericDbContext.GetAttachedEntity(c);
                if (attachedEntity==null)
                    genericDbContext.Attach(current);
                else
                {
                    return (T)attachedEntity;
                }
            }
            return current;
        }

        public void Dispose()
        {
            _asyncEnumerator.Dispose();
        }
    }
#endif
}
