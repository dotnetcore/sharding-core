using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Sharding.MergeEngines.AggregateMergeEngines
{
    public class AverageResult<T>
    {
        public AverageResult(T sum, long count)
        {
            Sum = sum;
            Count = count;
        }

        public T Sum { get; }
        public long Count { get; }

    }
}
