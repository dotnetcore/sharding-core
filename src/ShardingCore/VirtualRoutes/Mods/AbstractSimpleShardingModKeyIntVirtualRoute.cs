using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Mods
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 08:21:25
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分表字段为int的取模分表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractSimpleShardingModKeyIntVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,int> where T:class,IShardingEntity
    {
        protected readonly int Mod;
        protected AbstractSimpleShardingModKeyIntVirtualRoute(int mod)
        {
            if (mod < 1)
                throw new ArgumentException($"{nameof(mod)} less than 1 ");
            Mod = mod;
        }
        protected override int ConvertToShardingKey(object shardingKey)
        {
            return Convert.ToInt32(shardingKey);
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyInt = ConvertToShardingKey(shardingKey);
            return Math.Abs(shardingKeyInt % Mod).ToString();
        }

        public override List<string> GetAllTails()
        {
            return Enumerable.Range(0, Mod).Select(o => o.ToString()).ToList();
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(int shardingKey, ShardingOperatorEnum shardingOperator)
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