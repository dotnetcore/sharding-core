using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.EntityQueryConfigurations
{
    public class SeqQueryOrderMatch
    {
        public SeqQueryOrderMatch(bool isSameAsShardingTailComparer, SeqOrderMatchEnum orderMatch)
        {
            IsSameAsShardingTailComparer = isSameAsShardingTailComparer;
            OrderMatch = orderMatch;
        }

        public bool IsSameAsShardingTailComparer { get; }
        public SeqOrderMatchEnum OrderMatch { get; }
    }
}
