using System;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class EntityQueryMetadata
    {
        private static readonly IDictionary<QueryableMethodNameEnum, string> MethodNameSupports;

        static EntityQueryMetadata()
        {
            MethodNameSupports = new Dictionary<QueryableMethodNameEnum, string>()
            {
                { QueryableMethodNameEnum.First, nameof(Queryable.First) },
                { QueryableMethodNameEnum.FirstOrDefault, nameof(Queryable.FirstOrDefault) },
                { QueryableMethodNameEnum.Last, nameof(Queryable.Last) },
                { QueryableMethodNameEnum.LastOrDefault, nameof(Queryable.LastOrDefault) },
                { QueryableMethodNameEnum.Single, nameof(Queryable.Single) },
                { QueryableMethodNameEnum.SingleOrDefault, nameof(Queryable.SingleOrDefault) },
                { QueryableMethodNameEnum.Any, nameof(Queryable.Any) },
                { QueryableMethodNameEnum.All, nameof(Queryable.All) },
                { QueryableMethodNameEnum.Contains, nameof(Queryable.Contains) }
            };
        }

        private readonly IDictionary<string,bool> _seqQueryOrders;
        public IComparer<string> DefaultTailComparer { get;  set; }

        private readonly IDictionary<string, int> _seqConnectionsLimit;
        private readonly IDictionary<string, bool> _seqQueryDefaults;
        /// <summary>
        /// 
        /// </summary>
        public EntityQueryMetadata()
        {
            _seqQueryOrders = new Dictionary<string, bool>();
            DefaultTailComparer =Comparer<string>.Default;
            _seqConnectionsLimit = new Dictionary<string, int>();
            _seqQueryDefaults = new Dictionary<string, bool>();
        }

        /// <summary>
        /// 添加和默认数据库排序一样的排序
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="isAsc"></param>
        public void AddSeqComparerOrder(string propertyName,bool isAsc)
        {
            if (_seqQueryOrders.ContainsKey(propertyName))
            {
                _seqQueryOrders[propertyName] = isAsc;
            }
            else
            {
                _seqQueryOrders.Add(propertyName, isAsc);
            }
        }
        /// <summary>
        /// 添加对应方法的连接数限制
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="methodNameEnum"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddConnectionsLimit(int limit, QueryableMethodNameEnum methodNameEnum)
        {
            if (!MethodNameSupports.TryGetValue(methodNameEnum, out var methodName))
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
        /// <param name="asc"></param>
        /// <returns></returns>
        public bool TryContainsComparerOrder(string propertyName,out bool asc)
        {
            if (_seqQueryOrders.TryGetValue(propertyName, out var v))
            {
                asc = v;
                return true;
            }
            asc = false;
            return false;
        }

        /// <summary>
        /// 默认顺序查询熔断
        /// </summary>
        /// <param name="asc"></param>
        /// <param name="methodNameEnum"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddDefaultSequenceQueryTrip(bool asc,QueryableMethodNameEnum methodNameEnum)
        {
            if (!MethodNameSupports.TryGetValue(methodNameEnum, out var methodName))
            {
                throw new ArgumentException(methodNameEnum.ToString());
            }

            if (_seqQueryDefaults.ContainsKey(methodName))
            {
                _seqQueryDefaults[methodName] = asc;
            }
            else
            {
                _seqQueryDefaults.Add(methodName, asc);
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
