using System.Linq;
using ShardingCore.Core.VirtualDatabase.VirtualTables;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:54:52
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleContext
    {

        public TableRouteRuleContext(IQueryable queryable)
        {
            Queryable = queryable;
        }

        public IQueryable Queryable { get; }

    }
}