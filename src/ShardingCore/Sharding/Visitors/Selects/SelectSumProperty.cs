using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors.Selects
{
    public class SelectSumProperty:SelectAggregateProperty
    {
        public PropertyInfo FromProperty { get; }

        public SelectSumProperty(Type ownerType, PropertyInfo property,PropertyInfo fromProperty, bool isAggregateMethod, string aggregateMethod) : base(ownerType, property, isAggregateMethod, aggregateMethod)
        {
            FromProperty = fromProperty;
        }
    }
}
