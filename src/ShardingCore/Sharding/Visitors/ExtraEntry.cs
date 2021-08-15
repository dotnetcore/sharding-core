using System.Collections.Generic;
using ShardingCore.Core.Internal.Visitors.GroupBys;
using ShardingCore.Core.Internal.Visitors.Selects;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 13:13:45
* @Email: 326308290@qq.com
*/
    internal class ExtraEntry
    {
        public ExtraEntry(int? skip, int? take, IEnumerable<PropertyOrder> orders, SelectContext selectContext, GroupByContext groupByContext)
        {
            Skip = skip;
            Take = take;
            Orders = orders;
            SelectContext = selectContext;
            GroupByContext = groupByContext;
        }

        public int? Skip { get; }
        public int? Take { get; }
        public IEnumerable<PropertyOrder> Orders { get; }
        public SelectContext SelectContext { get; }
        public GroupByContext GroupByContext { get; }
    }
}