using System;

namespace ShardingCore.Core.Internal.Visitors.Selects
{
/*
* @Author: xjm
* @Description:
* @Date: Tuesday, 02 February 2021 08:17:48
* @Email: 326308290@qq.com
*/
    internal class SelectProperty
    {
        public SelectProperty(string propertyName,bool isAggregateMethod,string aggregateMethod)
        {
            PropertyName = propertyName;
            IsAggregateMethod = isAggregateMethod;
            AggregateMethod = aggregateMethod;
        }

        public string PropertyName { get; }
        public bool IsAggregateMethod { get; }
        public string AggregateMethod { get; }
    }
}