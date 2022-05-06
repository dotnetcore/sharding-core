using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;

namespace ShardingCore.Sharding.Enumerators.StreamMergeAsync
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:49:09
    * @Email: 326308290@qq.com
    */
    internal class MultiOrderStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {

        private readonly StreamMergeContext _mergeContext;
        private readonly IEnumerable<IStreamMergeAsyncEnumerator<T>> _enumerators;
        private readonly PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>> _queue;
        private IStreamMergeAsyncEnumerator<T> _currentEnumerator;
        private bool skipFirst;

        public MultiOrderStreamMergeAsyncEnumerator(StreamMergeContext mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> enumerators)
        {
            _mergeContext = mergeContext;
            _enumerators = enumerators;
            _queue = new PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>>(enumerators.Count());
            skipFirst = true;
            SetOrderEnumerator();
        }

        private void SetOrderEnumerator()
        {
            foreach (var source in _enumerators)
            {
                var orderStreamEnumerator = new OrderStreamMergeAsyncEnumerator<T>(_mergeContext, source);
                if (orderStreamEnumerator.HasElement())
                {
                    orderStreamEnumerator.SkipFirst();
                    _queue.Offer(orderStreamEnumerator);
                }
            }

            _currentEnumerator = _queue.IsEmpty() ? _enumerators.FirstOrDefault() : _queue.Peek();
        }
#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
#endif
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
#endif
        {
            if (_queue.IsEmpty())
                return false;
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }

            var first = _queue.Poll();
#if !EFCORE2

            if (await first.MoveNextAsync())
#endif
#if EFCORE2

            if (await first.MoveNext(cancellationToken))
#endif
            {
                _queue.Offer(first);
            }

            if (_queue.IsEmpty())
            {
                return false;
            }

            _currentEnumerator = _queue.Peek();
            return true;
        }
        public bool MoveNext()
        {
            if (_queue.IsEmpty())
                return false;
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }

            var first = _queue.Poll();
            if (first.MoveNext())
            {
                _queue.Offer(first);
            }

            if (_queue.IsEmpty())
            {
                return false;
            }

            _currentEnumerator = _queue.Peek();
            return true;
        }


        public bool SkipFirst()
        {
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }
            return false;
        }

        public bool HasElement()
        {
            return _currentEnumerator != null && _currentEnumerator.HasElement();
        }

        public T ReallyCurrent => _queue.IsEmpty() ? default(T) : _queue.Peek().ReallyCurrent;
        public T GetCurrent()
        {
            return _currentEnumerator.GetCurrent();
        }

#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var enumerator in _enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }
#endif



        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => skipFirst ? default : _currentEnumerator.GetCurrent();
        public void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}