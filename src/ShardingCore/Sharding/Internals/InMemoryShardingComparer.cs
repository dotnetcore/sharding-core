using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingComparision.Abstractions;

namespace ShardingCore.Sharding.Internals
{
    /// <summary>
    /// 内存分片比较器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InMemoryShardingComparer<T> : IComparer<T>
    {
        private readonly IShardingComparer _shardingComparer;

        public InMemoryShardingComparer(IShardingComparer shardingComparer)
        {
            _shardingComparer = shardingComparer;
        }
        public int Compare(T x, T y)
        {
            if (x is IComparable a && y is IComparable b)
                return _shardingComparer.Compare(a, b, true);
            throw new NotImplementedException($"compare :[{typeof(T).FullName}] is not IComparable");
        }
    }
}
