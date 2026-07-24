using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Extensions;
using ShardingCore.Sharding.MergeContexts;

namespace ShardingCore.Sharding.Enumerators
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Sunday, 15 August 2021 06:46:32
    * @Email: 326308290@qq.com
    */
    internal class OrderStreamMergeAsyncEnumerator<T> : IOrderStreamMergeAsyncEnumerator<T>
    {

        /// <summary>
        /// 合并数据上下文
        /// </summary>
        private readonly StreamMergeContext _mergeContext;

        private readonly IStreamMergeAsyncEnumerator<T> _enumerator;
        private List<IComparable> _orderValues;

        public OrderStreamMergeAsyncEnumerator(StreamMergeContext mergeContext, IStreamMergeAsyncEnumerator<T> enumerator)
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
#endif
#if EFCORE2
        public async Task<bool> MoveNext(CancellationToken cancellationToken = new CancellationToken())
#endif
        {
#if !EFCORE2
            var has = await _enumerator.MoveNextAsync();
#endif
#if EFCORE2
            var has = await _enumerator.MoveNext(cancellationToken);
#endif
            SetOrderValues();
            return has;
        }


        public bool MoveNext()
        {
            var has = _enumerator.MoveNext();
            SetOrderValues();
            return has;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public T Current => GetCurrent();

        public void Dispose()
        {
            _enumerator.Dispose();
        }

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

        private List<IComparable> GetCurrentOrderValues()
        {
            if (!_mergeContext.Orders.Any())
                return new List<IComparable>(0);
            var list = new List<IComparable>(_mergeContext.Orders.Count());
            foreach (var order in _mergeContext.Orders)
            {
                //标量投影查询(eg:OrderBy(x=>x.Id).Select(x=>x.Id))结果元素本身就是排序值
                if (TryGetScalarSelectOrderValue(_enumerator.ReallyCurrent, order, out var scalarOrderValue))
                {
                    list.Add(scalarOrderValue);
                    continue;
                }
                var (propertyType,value) = _enumerator.ReallyCurrent.GetValueByExpression(order.PropertyExpression);
                if (value is IComparable comparable)
                    list.Add(comparable);
                else if (propertyType.IsComparableType())
                {
                    list.Add((IComparable)value);
                }
                else if (value == null) // Support Nullable<xx>
                {
                    list.Add(null);
                }
                else
                {
                    throw new NotSupportedException($"order by value [{order}] must  implements IComparable");  
                }
            }

            return list;
        }

        /// <summary>
        /// 当前查询为标量投影(eg:Select(x=>x.Id))且投影属性与排序属性一致时,结果元素本身就是排序值
        /// </summary>
        private bool TryGetScalarSelectOrderValue(T reallyCurrent, PropertyOrder order, out IComparable orderValue)
        {
            orderValue = null;
            var selectProperties = _mergeContext.SelectContext.SelectProperties;
            if (selectProperties.Count != 1)
                return false;
            if (!string.Equals(selectProperties[0].PropertyName, order.PropertyExpression, StringComparison.OrdinalIgnoreCase))
                return false;
            //元素类型本身包含排序属性说明投影结果是实体或匿名类型,不能按标量处理
            if (reallyCurrent != null && reallyCurrent.GetType().GetUltimateShadowingProperty(order.PropertyExpression) != null)
                return false;
            if (reallyCurrent == null)
            {
                orderValue = null;
                return true;
            }
            if (reallyCurrent is IComparable comparable)
            {
                orderValue = comparable;
                return true;
            }
            return false;
        }

        public int CompareTo(IOrderStreamMergeAsyncEnumerator<T> other)
        {
            int i = 0;
            foreach (var order in _mergeContext.Orders)
            {
                int result = _mergeContext.GetShardingComparer().Compare(_orderValues[i], other.GetCompares()[i], order.IsAsc);
                if (0 != result)
                {
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
        public  ValueTask DisposeAsync()
        {
            return _enumerator.DisposeAsync();
        }
#endif

    }
}
