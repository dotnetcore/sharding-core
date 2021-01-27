using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;
using ShardingCore.Helpers;

namespace ShardingCore.VirtualRoutes.Mods
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 08:14:30
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分表字段为string的取模分表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractSimpleShardingModKeyStringVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,string> where T:class,IShardingEntity
    {
        protected readonly int Mod;
        protected AbstractSimpleShardingModKeyStringVirtualRoute(int mod)
        {
            Mod = mod;
        }
        /// <summary>
        /// 如何将shardingkey转成对应的tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyStr = ConvertToShardingKey(shardingKey);
            return Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % Mod).ToString();
        }
        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");           
#endif
                    return tail => true;
                }
            }
        }
    }
}