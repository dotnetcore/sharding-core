using System;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ShardingCore.Core.VirtualRoutes.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 19 December 2020 13:52:32
* @Email: 326308290@qq.com
*/
    public abstract class SimpleShardingKeyStringModVirtualRoute<T>:AbstractShardingKeyObjectEqualVirtualRoute<T,string> where T:class,IShardingEntity
    {

        private readonly int _mod;
        protected SimpleShardingKeyStringModVirtualRoute(int mod,ILogger<AbstractShardingKeyObjectEqualVirtualRoute<T, string>> _logger) : base(_logger)
        {
            _mod = mod;
        }
        protected override Expression<Func<string, bool>> GetRouteEqualToFilter(string shardingKey)
        {
            var modKey = ShardingKeyToTail(shardingKey);
            return s => s == modKey;
        }

        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyStr = ConvertToShardingKey(shardingKey);
            var bytes = Encoding.Default.GetBytes(shardingKeyStr);
            return Math.Abs(BitConverter.ToInt32(bytes,0) % _mod).ToString();
        }
    }
}