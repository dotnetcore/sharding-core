using System;
using System.Collections.Generic;
using ShardingCore.Sharding.ReadWriteConfigurations;

namespace ShardingCore.Core.ShardingConfigurations
{
    public class ShardingReadWriteSeparationOptions
    {
        public Func<IServiceProvider, IDictionary<string, IEnumerable<string>>> ReadWriteSeparationConfigure { get; set; }
        public Func<IServiceProvider, IDictionary<string, IEnumerable<ReadNode>>> ReadWriteNodeSeparationConfigure { get; set; }

        public ReadStrategyEnum ReadStrategy { get; set; } = ReadStrategyEnum.Loop;
        public bool DefaultEnable { get; set; } = false;
        public int DefaultPriority { get; set; } = 10;

        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; set; } =
            ReadConnStringGetStrategyEnum.LatestFirstTime;
    }
}
