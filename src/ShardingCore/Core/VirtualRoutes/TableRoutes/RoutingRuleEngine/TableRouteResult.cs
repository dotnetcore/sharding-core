using System;
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
        public TableRouteResult(IEnumerable<IPhysicTable> replaceTables, Type shardingDbContextType)
        {
            ShardingDbContextType = shardingDbContextType;
            ReplaceTables = replaceTables.ToHashSet();
        }
        
        public ISet<IPhysicTable> ReplaceTables { get; }
        public Type ShardingDbContextType { get; }
        protected bool Equals(TableRouteResult other)
        {
            return Equals(ReplaceTables, other.ReplaceTables) && Equals(ShardingDbContextType, other.ShardingDbContextType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableRouteResult)obj);
        }

#if !EFCORE2

        public override int GetHashCode()
        {
            return HashCode.Combine(ReplaceTables, ShardingDbContextType);
        }
#endif

#if EFCORE2

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ReplaceTables != null ? ReplaceTables.GetHashCode() : 0) * 397) ^ (ShardingDbContextType != null ? ShardingDbContextType.GetHashCode() : 0);
            }
        }
#endif
    }
}