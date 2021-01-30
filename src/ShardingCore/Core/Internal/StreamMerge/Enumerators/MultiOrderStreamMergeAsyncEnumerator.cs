using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Helpers;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 20:01:29
* @Email: 326308290@qq.com
*/
    internal class MultiOrderStreamMergeAsyncEnumerator<T> : IStreamMergeAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IEnumerable<IStreamMergeAsyncEnumerator<T>> _sources;
        private readonly PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>> _queue;
        private IStreamMergeAsyncEnumerator<T> _currentEnumerator;
        private bool skipFirst;

        public MultiOrderStreamMergeAsyncEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> sources)
        {
            _mergeContext = mergeContext;
            _sources = sources;
            _queue = new PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>>(sources.Count(),true);
            skipFirst = true;
            SetOrderEnumerator();
        }

        private void SetOrderEnumerator()
        {
            foreach (var source in _sources)
            {
                var orderStreamEnumerator = new OrderStreamMergeAsyncEnumerator<T>(_mergeContext, source);
                if (orderStreamEnumerator.HasElement())
                {
                    orderStreamEnumerator.SkipFirst();
                    _queue.Offer(orderStreamEnumerator);
                }
            }

            _currentEnumerator = _queue.IsEmpty() ? _sources.FirstOrDefault() : _queue.Peek();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_queue.IsEmpty())
                return false;
            if (skipFirst)
            {
                skipFirst = false;
                return true;
            }

            var first = _queue.Poll();
            if (await first.MoveNextAsync())
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

        public T ReallyCurrent => _queue.IsEmpty()?default(T):_queue.Peek().ReallyCurrent;

        public async ValueTask DisposeAsync()
        {
            foreach (var source in _sources)
            {
                await source.DisposeAsync();
            }
        }

        public T Current => skipFirst ? default : _currentEnumerator.Current;
    }
}