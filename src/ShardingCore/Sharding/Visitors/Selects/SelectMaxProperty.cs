﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.Visitors.Selects
{
    public class SelectMaxProperty:SelectAggregateProperty
    {
        public SelectMaxProperty(Type ownerType, PropertyInfo property, bool isAggregateMethod, string aggregateMethod) : base(ownerType, property, isAggregateMethod, aggregateMethod)
        {
        }
    }
}
