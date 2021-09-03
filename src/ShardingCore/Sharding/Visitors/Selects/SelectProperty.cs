using System;

namespace ShardingCore.Core.Internal.Visitors.Selects
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Tuesday, 02 February 2021 08:17:48
    * @Email: 326308290@qq.com
    */
    public class SelectProperty
    {
        public SelectProperty( Type ownerType,string propertyName,bool isAggregateMethod,string aggregateMethod)
        {
            OwnerType = ownerType;
            PropertyName = propertyName;
            IsAggregateMethod = isAggregateMethod;
            AggregateMethod = aggregateMethod;
        }
        public Type OwnerType { get; }
        public string PropertyName { get; }
        public bool IsAggregateMethod { get; }
        public string AggregateMethod { get; }
    }
}