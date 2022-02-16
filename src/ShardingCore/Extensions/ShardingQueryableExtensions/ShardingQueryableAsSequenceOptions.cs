using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Extensions.ShardingQueryableExtensions
{
    internal class ShardingQueryableAsSequenceOptions
    {
        public bool SameWithShardingComparer { get; }
        public bool AsSequence { get; }

        public ShardingQueryableAsSequenceOptions(bool sameWithShardingComparer,bool asSequence)
        {
            SameWithShardingComparer = sameWithShardingComparer;
            AsSequence = asSequence;
        }
    }
}
