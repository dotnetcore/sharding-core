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
        int? GetSkip();
        int? GetTake();
        
        PropertyOrder[] GetOrders();
        
        SelectContext GetSelectContext();
        
        GroupByContext GetGroupByConteext();
    }
}