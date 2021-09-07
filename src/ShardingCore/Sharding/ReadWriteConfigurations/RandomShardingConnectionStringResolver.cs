using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/6 14:22:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class RandomShardingConnectionStringResolver<TShardingDbContext> :IShardingConnectionStringResolver
    {
        public Type ShardingDbContextType => typeof(TShardingDbContext);

        private readonly string[] _connectionStrings;
        private readonly int _length;
        public RandomShardingConnectionStringResolver(IEnumerable<string> connectionStrings)
        {
            _connectionStrings = connectionStrings.ToArray();
            _length = _connectionStrings.Length;
        }
        public string GetConnectionString()
        {
            var next = RandomHelper.Next(0, _length);
            return _connectionStrings[next];

        }
    }
}
