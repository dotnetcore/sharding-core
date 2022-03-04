using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors.Selects
{
    public class SelectProperty
    {
        public SelectProperty( PropertyInfo property)
        {
            Property = property;
        }
        public PropertyInfo Property { get; }
        public string PropertyName => Property.Name;
    }
}
