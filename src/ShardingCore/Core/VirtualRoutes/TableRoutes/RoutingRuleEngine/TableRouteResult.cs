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
    public class TableRouteResult
    {
        public TableRouteResult(IEnumerable<IPhysicTable> replaceTables)
        {
            ReplaceTables = replaceTables.ToHashSet();
        }
        
        public ISet<IPhysicTable> ReplaceTables { get; }

        protected bool Equals(TableRouteResult other)
        {
            return Equals(ReplaceTables, other.ReplaceTables);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableRouteResult) obj);
        }

        public override int GetHashCode()
        {
            return (ReplaceTables != null ? ReplaceTables.GetHashCode() : 0);
        }
    }
}