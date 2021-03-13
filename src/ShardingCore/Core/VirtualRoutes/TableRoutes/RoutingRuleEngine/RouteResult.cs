using System.Collections.Generic;
using System.Linq;
using ShardingCore.Core.PhysicTables;

#if !EFCORE5
using ShardingCore.Extensions;
#endif

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:18:09
* @Email: 326308290@qq.com
*/
    public class RouteResult
    {
        public RouteResult(IEnumerable<IPhysicTable> replaceTables)
        {
            ReplaceTables = replaceTables.ToHashSet();
        }
        
        public ISet<IPhysicTable> ReplaceTables { get; }
    }
}