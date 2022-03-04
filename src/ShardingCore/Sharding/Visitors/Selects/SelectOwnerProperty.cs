using System;
using System.Reflection;
using ShardingCore.Sharding.Visitors.Selects;

/*
* @Author: xjm
* @Description:±í´ïselectÊôÐÔ 
* @Date: Tuesday, 02 February 2021 08:17:48
* @Email: 326308290@qq.com
*/
namespace ShardingCore.Core.Internal.Visitors.Selects
{
    public class SelectOwnerProperty: SelectProperty
    {
        public SelectOwnerProperty(Type ownerType, PropertyInfo property) : base(property)
        {
            OwnerType = ownerType;
            Property = property;
        }
        public Type OwnerType { get; }
        public PropertyInfo Property { get; }
        public string PropertyName => Property.Name;
    }
}