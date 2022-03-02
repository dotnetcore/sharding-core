using System;

namespace ShardingCore.Sharding.MergeContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 13 January 2021 15:35:15
    * @Email: 326308290@qq.com
    */
    public class PropertyOrder
    {
        public PropertyOrder(string propertyExpression, bool isAsc,Type ownerType)
        {
            PropertyExpression = propertyExpression;
            IsAsc = isAsc;
            OwnerType = ownerType;
        }

        public string PropertyExpression { get; set; }
        public bool IsAsc { get; set; }
        public Type OwnerType { get; }

        public override string ToString()
        {
            return $"{PropertyExpression} {(IsAsc ? "asc" : "desc")}";
        }
    }
}