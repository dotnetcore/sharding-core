using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Enumerators.TrackerEnumerators;

namespace ShardingCore.Sharding.Enumerators.TrackerEnumerables
{
    public class TrackEnumerable<T>:IEnumerable<T>
    {
        private readonly IShardingDbContext _shardingDbContext;
        private readonly IEnumerable<T> _enumerable;

        public TrackEnumerable(IShardingDbContext shardingDbContext,IEnumerable<T> enumerable)
        {
            _shardingDbContext = shardingDbContext;
            _enumerable = enumerable;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new TrackerEnumerator<T>(_shardingDbContext,_enumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
