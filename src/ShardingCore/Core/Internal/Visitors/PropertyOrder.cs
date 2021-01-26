namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 15:35:15
* @Email: 326308290@qq.com
*/
    internal class PropertyOrder
    {
        public PropertyOrder(string propertyExpression, bool isAsc)
        {
            PropertyExpression = propertyExpression;
            IsAsc = isAsc;
        }

        public string PropertyExpression { get; set; }
        public bool IsAsc { get; set; }
        public override string ToString()
        {
            return $"{PropertyExpression} {(IsAsc ? "asc" : "desc")}";
        }
    }
}