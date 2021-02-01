using System;

namespace ShardingCore.Core.Internal.Visitors.GroupBys
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 01 February 2021 21:20:19
* @Email: 326308290@qq.com
*/
    public class GroupByAggregateMethod
    {
        public GroupByAggregateMethod(string aggregateMethodName)
        {
            AggregateMethodName = aggregateMethodName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string AggregateMethodName { get; }
    }
}