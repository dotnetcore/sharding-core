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
        public ISet<string> SeqQueryOrders { get; }
        public IComparer<string> DefaultTailComparer { get; set; }

        public IDictionary<string,int> SeqConnectionsLimit { get; }

        public EntityQueryMetadata()
        {
            SeqQueryOrders = new HashSet<string>();
            DefaultTailComparer=Comparer<string>.Default;
            SeqConnectionsLimit = new Dictionary<string, int>();
        }

        public void AddConnectionsLimit(int limit, QueryableMethodNameEnum methodNameEnum)
        {
            if (!MethodNameSupports.TryGetValue(methodNameEnum, out var methodName))
            {
                throw new ArgumentException(methodNameEnum.ToString());
            }

            if (SeqConnectionsLimit.ContainsKey(methodName))
            {
                SeqConnectionsLimit[methodName]= limit;
            }
            else
            {
                SeqConnectionsLimit.Add(methodName,limit);
            }
        }

        public bool TryGetConnectionsLimit(string methodName,out int limit)
        {
            if (SeqConnectionsLimit.TryGetValue(methodName, out var l))
            {
                limit = l;
                return true;
            }

            limit = 0;
            return false;
        }

        public bool ContainsComparerOrder(string propertyName)
        {
            return SeqQueryOrders.Contains(propertyName);
        }

    }
}
