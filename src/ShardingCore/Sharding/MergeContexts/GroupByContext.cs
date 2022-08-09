using System.Collections.Generic;
using System.Linq.Expressions;

namespace ShardingCore.Sharding.MergeContexts
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 01 February 2021 21:17:55
    * @Email: 326308290@qq.com
    */
    public class GroupByContext
    {
        /// <summary>
        /// group by 表达式
        /// </summary>
        public LambdaExpression GroupExpression { get; set; }
        /// <summary>
        /// 是否内存聚合
        /// </summary>
        public bool GroupMemoryMerge { get; set; }
        public List<PropertyOrder> PropertyOrders { get; } = new List<PropertyOrder>();
        public string GetOrderExpression()
        {
            return string.Join(",", PropertyOrders);
        }

    }
}