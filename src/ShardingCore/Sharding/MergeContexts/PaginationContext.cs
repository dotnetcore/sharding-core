using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeContexts
{
    public sealed class PaginationContext
    {
        public int? Skip { get;  set; }
        public int? Take { get;  set; }

        public bool HasSkip()
        {
            return Skip.HasValue;
        }
        public bool HasTake()
        {
            return Take.HasValue;
        }
    }
}
