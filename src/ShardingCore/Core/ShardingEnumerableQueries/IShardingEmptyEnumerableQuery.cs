using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.ShardingEnumerableQueries
{
    public interface IShardingEmptyEnumerableQuery
    {
        IQueryable EmptyQueryable();
    }
}
