using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class EntityQueryMetadata
    {
        public const string QUERY_ENUMERATOR = "Enumerator";
        private static readonly IDictionary<CircuitBreakerMethodNameEnum, string> CircuitBreakerMethodNameSupports;
        private static readonly IDictionary<LimitMethodNameEnum, string> LimitMethodNameSupports;

        static EntityQueryMetadata()
        {
            CircuitBreakerMethodNameSupports = new Dictionary<CircuitBreakerMethodNameEnum, string>()
            {
                { CircuitBreakerMethodNameEnum.First, nameof(Queryable.First) },
                { CircuitBreakerMethodNameEnum.FirstOrDefault, nameof(Queryable.FirstOrDefault) },
                { CircuitBreakerMethodNameEnum.Last, nameof(Queryable.Last) },
                { CircuitBreakerMethodNameEnum.LastOrDefault, nameof(Queryable.LastOrDefault) },
                { CircuitBreakerMethodNameEnum.Single, nameof(Queryable.Single) },
                { CircuitBreakerMethodNameEnum.SingleOrDefault, nameof(Queryable.SingleOrDefault) },
                { CircuitBreakerMethodNameEnum.Any, nameof(Queryable.Any) },
                { CircuitBreakerMethodNameEnum.All, nameof(Queryable.All) },
                { CircuitBreakerMethodNameEnum.Contains, nameof(Queryable.Contains) },
                { CircuitBreakerMethodNameEnum.Enumerator,QUERY_ENUMERATOR }
            };
            LimitMethodNameSupports = new Dictionary<LimitMethodNameEnum, string>()
            {
                { LimitMethodNameEnum.First, nameof(Queryable.First) },
                { LimitMethodNameEnum.FirstOrDefault, nameof(Queryable.FirstOrDefault) },
                { LimitMethodNameEnum.Last, nameof(Queryable.Last) },
                { LimitMethodNameEnum.LastOrDefault, nameof(Queryable.LastOrDefault) },
                { LimitMethodNameEnum.Single, nameof(Queryable.Single) },
                { LimitMethodNameEnum.SingleOrDefault, nameof(Queryable.SingleOrDefault) },
                { LimitMethodNameEnum.Any, nameof(Queryable.Any) },
                { LimitMethodNameEnum.All, nameof(Queryable.All) },
                { LimitMethodNameEnum.Contains, nameof(Queryable.Contains) },
                { LimitMethodNameEnum.Max, nameof(Queryable.Max) },
                { LimitMethodNameEnum.Min, nameof(Queryable.Min) },
                { LimitMethodNameEnum.Count, nameof(Queryable.Count) },
                { LimitMethodNameEnum.LongCount, nameof(Queryable.LongCount) },
                { LimitMethodNameEnum.Sum, nameof(Queryable.Sum) },
                { LimitMethodNameEnum.Average, nameof(Queryable.Average) },
                { LimitMethodNameEnum.Enumerator, QUERY_ENUMERATOR }
            };
        }

        private readonly IDictionary<string, SeqQueryOrderMatch> _seqQueryOrders;
        public IComparer<string> DefaultTailComparer { get;  set; }
        public bool DefaultTailComparerNeedReverse { get;  set; }

        private readonly IDictionary<string, int> _seqConnectionsLimit;
        private readonly IDictionary<string, bool> _seqQueryDefaults;
        /// <summary>
        /// 
        /// </summary>
        public EntityQueryMetadata()
        {
            _seqQueryOrders = new Dictionary<string, SeqQueryOrderMatch>();
            DefaultTailComparer =Comparer<string>.Default;
            DefaultTailComparerNeedReverse = true;
            _seqConnectionsLimit = new Dictionary<string, int>();
            _seqQueryDefaults = new Dictionary<string, bool>();
        }

        /// <summary>
        /// 添加和默认数据库排序一样的排序
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="isSameAsShardingTailComparer"></param>
        /// <param name="seqOrderMatch"></param>
        public void AddSeqComparerOrder(string propertyName,bool isSameAsShardingTailComparer,SeqOrderMatchEnum seqOrderMatch)
        {
            if (_seqQueryOrders.ContainsKey(propertyName))
            {
                _seqQueryOrders[propertyName] = new SeqQueryOrderMatch(isSameAsShardingTailComparer,seqOrderMatch);
            }
            else
            {
                _seqQueryOrders.Add(propertyName, new SeqQueryOrderMatch(isSameAsShardingTailComparer, seqOrderMatch));
            }
        }
        /// <summary>
        /// 添加对应方法的连接数限制
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="methodNameEnum"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddConnectionsLimit(int limit, LimitMethodNameEnum methodNameEnum)
        {
            if (!LimitMethodNameSupports.TryGetValue(methodNameEnum, out var methodName))
            {
                throw new ArgumentException(methodNameEnum.ToString());
            }

            if (_seqConnectionsLimit.ContainsKey(methodName))
            {
                _seqConnectionsLimit[methodName]= limit;
            }
            else
            {
                _seqConnectionsLimit.Add(methodName,limit);
            }
        }
        /// <summary>
        /// 尝试获取当前查询方法配置的连接数限制
        /// </summary>
        /// <param name="methodName">First、FirstOrDefault...</param>
        /// <param name="limit">连接数限制</param>
        /// <returns></returns>
        public bool TryGetConnectionsLimit(string methodName,out int limit)
        {
            if (_seqConnectionsLimit.TryGetValue(methodName, out var l))
            {
                limit = l;
                return true;
            }

            limit = 0;
            return false;
        }
        /// <summary>
        /// 是否包含当前排序字段
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="seqQueryOrderMatch"></param>
        /// <returns></returns>
        public bool TryContainsComparerOrder(string propertyName,out SeqQueryOrderMatch seqQueryOrderMatch)
        {
            if (_seqQueryOrders.TryGetValue(propertyName, out var v))
            {
                seqQueryOrderMatch = v;
                return true;
            }
            seqQueryOrderMatch = null;
            return false;
        }

        /// <summary>
        /// 默认顺序查询熔断
        /// </summary>
        /// <param name="isSameAsShardingTailComparer"></param>
        /// <param name="methodNameEnum"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddDefaultSequenceQueryTrip(bool isSameAsShardingTailComparer, CircuitBreakerMethodNameEnum methodNameEnum)
        {
            if (!CircuitBreakerMethodNameSupports.TryGetValue(methodNameEnum, out var methodName))
            {
                throw new ArgumentException(methodNameEnum.ToString());
            }

            if (_seqQueryDefaults.ContainsKey(methodName))
            {
                _seqQueryDefaults[methodName] = isSameAsShardingTailComparer;
            }
            else
            {
                _seqQueryDefaults.Add(methodName, isSameAsShardingTailComparer);
            }
        }
        /// <summary>
        /// 当前方法是否配置了顺序排序查询熔断
        /// </summary>
        /// <param name="asc"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public bool TryGetDefaultSequenceQueryTrip(string methodName,out bool asc)
        {
            if (_seqQueryDefaults.TryGetValue(methodName, out var v))
            {
                asc = v;
                return true;
            }
            asc = false;
            return false;
        }

    }
}
