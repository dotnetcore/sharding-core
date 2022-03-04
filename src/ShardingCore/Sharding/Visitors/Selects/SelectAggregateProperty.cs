using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Core.Internal.Visitors.Selects;

namespace ShardingCore.Sharding.Visitors.Selects
{
    public class SelectAggregateProperty : SelectOwnerProperty
    {
        public SelectAggregateProperty(Type ownerType, PropertyInfo property, bool isAggregateMethod, string aggregateMethod):base(ownerType, property)
        {
            IsAggregateMethod = isAggregateMethod;
            AggregateMethod = aggregateMethod;
        }
        public bool IsAggregateMethod { get; }
        public string AggregateMethod { get; }
    }
}
