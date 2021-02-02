using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.PriorityQueues;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Core.Internal.StreamMerge.Enumerators.AggregateExtensions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 09:55:20
* @Email: 326308290@qq.com
*/
    internal class MultiAggregateOrderStreamMergeAsyncEnumerator<T>: IStreamMergeAsyncEnumerator<T>
    {
        private readonly StreamMergeContext<T> _mergeContext;
        private readonly IEnumerable<IStreamMergeAsyncEnumerator<T>> _enumerators;
        private readonly PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>> _queue;
        private T CurrentValue;
        private List<object> CurrentGroupValues;
        private bool _skipFirst;

        public MultiAggregateOrderStreamMergeAsyncEnumerator(StreamMergeContext<T> mergeContext, IEnumerable<IStreamMergeAsyncEnumerator<T>> enumerators)
        {
            _mergeContext = mergeContext;
            _enumerators = enumerators;
            _queue = new PriorityQueue<IOrderStreamMergeAsyncEnumerator<T>>(enumerators.Count());
            _skipFirst = true;
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
            //设置第一个元素聚合的属性值
            CurrentGroupValues = _queue.IsEmpty() ? new List<object>(0) : GetCurrentGroupValues(_queue.Peek());
        }

        private List<object> GetCurrentGroupValues(IOrderStreamMergeAsyncEnumerator<T> enumerator)
        {
            var first = enumerator.ReallyCurrent;
            return _mergeContext.SelectContext.SelectProperties.Where(o => !o.IsAggregateMethod)
                .Select(o => first.GetValueByExpression(o.PropertyName)).ToList();
        }
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken)
#endif
#if !EFCORE2
        public async ValueTask<bool> MoveNextAsync()
#endif
        {
            if (_queue.IsEmpty())
                return false;
#if EFCORE2
            var hasNext=await SetCurrentValue(cancellationToken);
#endif
#if !EFCORE2
            var hasNext=await SetCurrentValue();
#endif
            if (hasNext)
            {
                CurrentGroupValues = _queue.IsEmpty() ? new List<object>(0) : GetCurrentGroupValues(_queue.Peek());
            }
            return hasNext;
        }

        private bool EqualWithGroupValues()
        {
            var current = GetCurrentGroupValues(_queue.Peek());
            for (int i = 0; i < CurrentGroupValues.Count; i++)
            {
                if (!CurrentGroupValues[i].Equals(current[i]))
                    return false;
            }

            return true;
        }
#if EFCORE2
        private async Task<bool> SetCurrentValue(CancellationToken cancellationToken)
#endif
#if !EFCORE2
        private async ValueTask<bool>  SetCurrentValue()
#endif
        {
            CurrentValue = default;
            var currentValues = new List<T>();
            while (EqualWithGroupValues())
            {
                var current=_queue.Peek().Current;
                currentValues.Add(current);
                var first = _queue.Poll();
#if EFCORE2
            if (await first.MoveNext(cancellationToken))
#endif
#if !EFCORE2
                if (await first.MoveNextAsync())
#endif
                {
                    _queue.Offer(first);
                }

                if (_queue.IsEmpty())
                {
                    break;
                }
            }

            MergeValue(currentValues);

            return true;
        }

        private void MergeValue(List<T> aggregateValues)
        {
           
            if (aggregateValues.IsNotEmpty())
            {
                CurrentValue = aggregateValues.First();
                if (aggregateValues.Count > 1)
                {
                    var aggregates = _mergeContext.SelectContext.SelectProperties.Where(o => o.IsAggregateMethod).ToList();
                    if (aggregates.IsNotEmpty())
                    {
                        foreach (var aggregate in aggregates)
                        {
                            object aggregateValue = null;
                            if (aggregate.AggregateMethod == nameof(Queryable.Count))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Sum(aggregate.PropertyName);
                            } else if (aggregate.AggregateMethod == nameof(Queryable.Sum))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Sum(aggregate.PropertyName);
                            } else if (aggregate.AggregateMethod == nameof(Queryable.Max))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Max(aggregate.PropertyName);
                            }else if (aggregate.AggregateMethod == nameof(Queryable.Min))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Min(aggregate.PropertyName);
                            }else if (aggregate.AggregateMethod == nameof(Queryable.Average))
                            {
                                aggregateValue = aggregateValues.AsQueryable().Average(aggregate.PropertyName);
                            }
                            else
                            {
                                throw new InvalidOperationException($"method:{aggregate.AggregateMethod} invalid operation ");
                            }
                            CurrentValue.SetPropertyValue(aggregate.PropertyName,aggregateValue);
                        }
                    }
                }
            }
        }


        public bool SkipFirst()
        {
            return true;
        }

        public bool HasElement()
        {
            return ReallyCurrent != null;
        }

        public T ReallyCurrent => _queue.IsEmpty()?default(T):_queue.Peek().ReallyCurrent;

#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            foreach (var enumerator in _enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }
#endif

#if EFCORE2
        public  void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                 enumerator.Dispose();
            }
        }
#endif

        public T Current => CurrentValue;
    }
}