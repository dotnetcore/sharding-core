using System;
using System.Linq;
using ShardingCore.Core.Internal.Visitors.Selects;

namespace ShardingCore.Sharding.MergeContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 02 March 2022 21:13:35
* @Email: 326308290@qq.com
*/
    public interface IParseResult
    {
        PaginationContext GetPaginationContext();
        OrderByContext GetOrderByContext();
        
        SelectContext GetSelectContext();
        
        GroupByContext GetGroupByContext();
    }
}