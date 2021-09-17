using System.Linq;
using ShardingCore.Core.VirtualTables;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes.RoutingRuleEngine
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 28 January 2021 10:54:52
* @Email: 326308290@qq.com
*/
    public class TableRouteRuleContext<T>
    {
        private readonly IVirtualTableManager _virtualTableManager;

        public TableRouteRuleContext(string dsname, IQueryable<T> queryable, IVirtualTableManager virtualTableManager)
        {
            Dsname = dsname;
            Queryable = queryable;
            _virtualTableManager = virtualTableManager;
        }

        public string Dsname { get; }
        public IQueryable<T> Queryable { get; }

    }
}