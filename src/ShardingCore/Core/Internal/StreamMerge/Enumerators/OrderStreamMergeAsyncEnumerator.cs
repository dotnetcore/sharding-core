using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.StreamMerge.Abstractions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.StreamMerge.Enumerators
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 19:48:16
* @Email: 326308290@qq.com
*/
    internal class OrderStreamMergeAsyncEnumerator<T>:IOrderStreamMergeAsyncEnumerator<T>
    {
        /// <summary>
        /// 合并数据上下文
        /// </summary>
        private readonly StreamMergeContext<T> _mergeContext;

        private readonly IStreamMergeAsyncEnumerator<T> _enumerator;
        private List<IComparable> _orderValues;

        public OrderStreamMergeAsyncEnumerator(StreamMergeContext<T> mergeContext, IStreamMergeAsyncEnumerator<T> enumerator)
        {
            _mergeContext = mergeContext;
            _enumerator = enumerator;
            SetOrderValues();
        }

        private void SetOrderValues()
        {
            _orderValues = HasElement() ? GetCurrentOrderValues() : new List<IComparable>(0);
        }
        
#if !EFCORE2

        public async ValueTask<bool> MoveNextAsync()
        {
            var has = await _enumerator.MoveNextAsync();
            SetOrderValues();
            return has;
        }
#endif
        
#if EFCORE2

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            var has = await _enumerator.MoveNext(cancellationToken);
            SetOrderValues();
            return has;
        }
#endif

        public T Current =>_enumerator.Current;

        public bool SkipFirst()
        {
            return _enumerator.SkipFirst();
        }

        public bool HasElement()
        {
            return _enumerator.HasElement();
        }

        public T ReallyCurrent => _enumerator.ReallyCurrent;

        private List<IComparable> GetCurrentOrderValues()
        {
            if (!_mergeContext.Orders.Any())
                return new List<IComparable>(0);
            var list = new List<IComparable>(_mergeContext.Orders.Count());
            foreach (var order in _mergeContext.Orders)
            {
                var value = _enumerator.ReallyCurrent.GetValueByExpression(order.PropertyExpression);
                if (value is IComparable comparable)
                    list.Add(comparable);
                else
                    throw new NotSupportedException($"order by value [{order}] must  implements IComparable");
            }

            return list;
        }

        public int CompareTo(IOrderStreamMergeAsyncEnumerator<T> other)
        {
            int i = 0;
            foreach (var order in _mergeContext.Orders) {
                int result = CompareHelper.CompareToWith(_orderValues[i], other.GetCompares()[i], order.IsAsc);
                if (0 != result) {
                    return result;
                }
                i++;
            }
            return 0;
        }

        public List<IComparable> GetCompares()
        {
            return _orderValues ?? new List<IComparable>(0);
        }
#if !EFCORE2

        public async ValueTask DisposeAsync()
        {
            await _enumerator.DisposeAsync();
        }
#endif

#if EFCORE2
        public void Dispose()
        {
            _enumerator.Dispose();
        }
#endif
    }
}