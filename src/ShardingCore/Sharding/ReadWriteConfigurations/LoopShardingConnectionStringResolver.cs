using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 14:39:23
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class LoopShardingConnectionStringResolver<TShardingDbContext> : IShardingConnectionStringResolver
    {
        public Type ShardingDbContextType => typeof(TShardingDbContext);

        private readonly string[] _connectionStrings;
        private readonly int _length;
        private long _seed = 0;
        public LoopShardingConnectionStringResolver(IEnumerable<string> connectionStrings)
        {
            _connectionStrings = connectionStrings.ToArray();
            _length = _connectionStrings.Length;
        }
        public string GetConnectionString()
        {
            Interlocked.Increment(ref _seed);
            var next = (int)(_seed % _length);
            if (next < 0)
                return _connectionStrings[Math.Abs(next)];
            return _connectionStrings[next];
        }

    }
}
