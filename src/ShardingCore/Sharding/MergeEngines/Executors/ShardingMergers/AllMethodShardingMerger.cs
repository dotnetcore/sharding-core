using System.Collections.Generic;
using System.Linq;
using ShardingCore.Sharding.MergeEngines.ShardingMergeEngines.Abstractions;
using ShardingCore.Sharding.StreamMergeEngines;

namespace ShardingCore.Sharding.MergeEngines.Executors.ShardingMergers
{
    internal class AllMethodShardingMerger:IShardingMerger<bool>
    {
        private static readonly IShardingMerger<bool> _allShardingMerger;

        static AllMethodShardingMerger()
        {
            _allShardingMerger = new AllMethodShardingMerger();
        }

        public static IShardingMerger<bool> Instance => _allShardingMerger;
      
        public bool StreamMerge(List<bool> parallelResults)
        {
            return parallelResults.All(o => o);
        }

        public void InMemoryMerge(List<bool> beforeInMemoryResults, List<bool> parallelResults)
        {
            beforeInMemoryResults.AddRange(parallelResults);
        }
    }
}
