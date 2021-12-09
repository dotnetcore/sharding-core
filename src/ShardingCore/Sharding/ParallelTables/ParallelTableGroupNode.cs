using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.ParallelTables
{
    /// <summary>
    /// 平行表组节点用来表示一组平行表
    /// </summary>
    public class ParallelTableGroupNode
    {
        private readonly ISet<ParallelTableComparerType> _entities;


        public ParallelTableGroupNode(IEnumerable<ParallelTableComparerType> entities)
        {
            _entities = new SortedSet<ParallelTableComparerType>(entities);
        }

        public ISet<ParallelTableComparerType> GetEntities()
        {
            return _entities;
        }
        protected bool Equals(ParallelTableGroupNode other)
        {
            return _entities.SequenceEqual(other._entities);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParallelTableGroupNode)obj);
        }

        public override int GetHashCode()
        {
            return (_entities != null ? _entities.Sum(o=>o.GetHashCode()) : 0);
        }
    }
}
