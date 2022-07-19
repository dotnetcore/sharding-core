using System;
using System.Collections.Generic;
using System.Linq;
using ShardingCore.Extensions;


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
        public TableRouteResult(List<TableRouteUnit> replaceTables)
        {
            ReplaceTables = replaceTables.ToHashSet();
            HasDifferentTail = ReplaceTables.IsNotEmpty() && ReplaceTables.GroupBy(o => o.Tail).Count() != 1;
            IsEmpty = replaceTables.Count == 0;
        }
        public TableRouteResult(TableRouteUnit replaceTable):this(new List<TableRouteUnit>(){replaceTable})
        {
        }
        
        public ISet<TableRouteUnit> ReplaceTables { get; }

        public bool HasDifferentTail { get; }
        public bool IsEmpty { get; }

        protected bool Equals(TableRouteResult other)
        {
            return Equals(ReplaceTables, other.ReplaceTables) && HasDifferentTail == other.HasDifferentTail && IsEmpty == other.IsEmpty;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableRouteResult)obj);
        }

        public override string ToString()
        {
            return $"(has different tail:{HasDifferentTail},current table:[{string.Join(",", ReplaceTables.Select(o => $"{o.DataSourceName}.{o.Tail}.{o.EntityType}"))}])";
        }

#if !EFCORE2

        public override int GetHashCode()
        {
            return HashCode.Combine(ReplaceTables, HasDifferentTail, IsEmpty);
        }
#endif

#if EFCORE2

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ReplaceTables != null ? ReplaceTables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HasDifferentTail.GetHashCode();
                hashCode = (hashCode * 397) ^ IsEmpty.GetHashCode();
                return hashCode;
            }
        }
#endif
    }
}