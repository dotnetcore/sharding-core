using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:39:52
    * @Email: 326308290@qq.com
    */
    internal class PaginationStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private readonly StreamMergeContext _mergeContext;
        private readonly IStreamMergeAsyncEnumerator<T> _enumerator;
        private readonly int? _skip;
        private readonly int? _take;
        private int realSkip = 0;
        private int realTake = 0;

        public PaginationStreamMergeAsyncEnumerator(StreamMergeContext mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> sources):this(mergeContext, sources,mergeContext.Skip, mergeContext.Take)
        {
        }
        public PaginationStreamMergeAsyncEnumerator(StreamMergeContext mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> sources,int? skip,int? take)
        {
            _mergeContext = mergeContext;
            _skip = skip;
            _take = take;
            if (_mergeContext.HasGroupQuery())
                _enumerator = new MultiAggregateOrderStreamMergeAsyncEnumerator<T>(_mergeContext, sources);
            else
                _enumerator = new MultiOrderStreamMergeAsyncEnumerator<T>(_mergeContext, sources);
        }
#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
#endif
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
#endif
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            while (_skip.GetValueOrDefault() > this.realSkip)
            {
#if !EFCORE2
                var has = await _enumerator.MoveNextAsync();
#endif
#if EFCORE2
                var has = await _enumerator.MoveNext(cancellationToken);
#endif

                realSkip++;
                if (!has)
                    return false;
            }

#if !EFCORE2
            var next = await _enumerator.MoveNextAsync();
#endif
#if EFCORE2
            var next = await _enumerator.MoveNext(cancellationToken);
#endif

            if (next)
            {
                if (_take.HasValue)
                {
                    realTake++;
                    if (realTake > _take.Value)
                        return false;
                }
            }

            return next;
        }

        public bool MoveNext()
        {
            //如果合并数据的时候不需要跳过也没有take多少那么就是直接next
            while (_skip.GetValueOrDefault() > this.realSkip)
            {
                var has = _enumerator.MoveNext();
                realSkip++;
                if (!has)
                    return false;
            }

            var next = _enumerator.MoveNext();

            if (next)
            {
                if (_take.HasValue)
                {
                    realTake++;
                    if (realTake > _take.Value)
                        return false;
                }
            }

            return next;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => _enumerator.GetCurrent();
        public bool SkipFirst()
        {
            return _enumerator.SkipFirst();
        }

        public bool HasElement()
        {
            return _enumerator.HasElement();
        }

        public T ReallyCurrent => _enumerator.ReallyCurrent;
        public T GetCurrent()
        {
            return _enumerator.GetCurrent();
        }
        public void Dispose()
        {
            _enumerator.Dispose();
        }
#if !EFCORE2

        public ValueTask DisposeAsync()
        {
            return _enumerator.DisposeAsync();
        }
#endif
    }
}