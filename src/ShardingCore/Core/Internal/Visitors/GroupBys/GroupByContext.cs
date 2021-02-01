using System.Collections.Generic;
using System.Linq.Expressions;

namespace ShardingCore.Core.Internal.Visitors.GroupBys
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 21:17:55
* @Email: 326308290@qq.com
*/
    public class GroupByContext
    {

        public LambdaExpression GroupExpression { get; set; } 
        public List<GroupByAggregateMethod> GroupByAggregateMethods { get; set; } = new List<GroupByAggregateMethod>();

    }
}